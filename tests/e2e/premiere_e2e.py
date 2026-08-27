#!/usr/bin/env python3
from __future__ import annotations

import json
import os
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid

BASE = os.environ.get("JELLYFIN_URL", "http://127.0.0.1:8096").rstrip("/")
CLIENT_HEADER = 'MediaBrowser Client="JellyPremiere%20CI", DeviceId="jellypremiere-ci", Device="GitHub%20Actions", Version="1.0.1.0"'
ADMIN_NAME = "premiere-admin"
ADMIN_PASSWORD = "premiere-admin-password"
USER_NAME = "premiere-user"
USER_PASSWORD = "premiere-user-password"


def call(method: str, path: str, body=None, token: str | None = None, expected=(200, 204), raw=False):
    data = None
    headers = {"Accept": "application/json", "Authorization": CLIENT_HEADER + (f", Token={token}" if token else "")}
    if body is not None:
        data = json.dumps(body).encode("utf-8")
        headers["Content-Type"] = "application/json"
    req = urllib.request.Request(BASE + path, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=30) as response:
            payload = response.read()
            status = response.status
            content_type = response.headers.get("content-type", "")
    except urllib.error.HTTPError as exc:
        payload = exc.read()
        status = exc.code
        content_type = exc.headers.get("content-type", "")
    if status not in expected:
        raise AssertionError(f"{method} {path}: expected {expected}, got {status}: {payload[:1000]!r}")
    if raw:
        return status, payload, content_type
    if not payload:
        return None
    return json.loads(payload.decode("utf-8"))


def pick(obj: dict, name: str):
    return obj.get(name) if name in obj else obj.get(name[0].lower() + name[1:])


def wait_for_server():
    deadline = time.time() + 180
    last_error = None
    while time.time() < deadline:
        try:
            status, payload, _ = call("GET", "/System/Info/Public", expected=(200,), raw=True)
            if status == 200 and payload:
                return
        except Exception as exc:  # noqa: BLE001
            last_error = exc
        time.sleep(2)
    raise RuntimeError(f"Jellyfin did not start: {last_error}")


def authenticate(username: str, password: str) -> str:
    result = call("POST", "/Users/AuthenticateByName", {"Username": username, "Pw": password}, expected=(200,))
    token = pick(result, "AccessToken")
    if not token:
        raise AssertionError(f"No access token for {username}: {result}")
    return token


def assert_access_denied(method: str, path: str, token: str) -> int:
    # Depending on where Jellyfin 10.10.7 rejects a non-admin request, the
    # framework can return 401 before the controller or 403 from the controller.
    # Both are valid secure outcomes; success (2xx) must never be accepted.
    status, _, _ = call(method, path, token=token, expected=(401, 403), raw=True)
    if status not in (401, 403):
        raise AssertionError(f"Expected access denial for {method} {path}, got {status}")
    return status


def main() -> None:
    wait_for_server()
    initial_user = call("GET", "/Startup/User", expected=(200,))
    assert pick(initial_user, "Name")
    call("POST", "/Startup/Configuration", {
        "UICulture": "es-ES",
        "MetadataCountryCode": "ES",
        "PreferredMetadataLanguage": "es",
    }, expected=(204,))
    call("POST", "/Startup/User", {"Name": ADMIN_NAME, "Password": ADMIN_PASSWORD}, expected=(204,))
    call("POST", "/Startup/RemoteAccess", {"EnableRemoteAccess": False, "EnableAutomaticPortMapping": False}, expected=(204,))
    call("POST", "/Startup/Complete", expected=(204,))

    admin_token = authenticate(ADMIN_NAME, ADMIN_PASSWORD)
    _, index_bytes, content_type = call("GET", "/web/index.html", token=admin_token, expected=(200,), raw=True)
    index = index_bytes.decode("utf-8", errors="replace")
    assert "data-jellypremiere-client" in index, "JellyPremiere client script was not injected"
    assert "../JellyPremiere/ClientScript.js" in index, "JellyPremiere client script URL missing"
    assert "text/html" in content_type.lower()

    _, script_bytes, script_type = call("GET", "/JellyPremiere/ClientScript.js", expected=(200,), raw=True)
    script = script_bytes.decode("utf-8", errors="replace")
    assert "[JellyPremiere] Client initialized" in script
    assert "checkActiveAnnouncements" in script
    assert "javascript" in script_type.lower()

    created_user = call("POST", "/Users/New", {"Name": USER_NAME, "Password": USER_PASSWORD}, token=admin_token, expected=(200,))
    assert pick(created_user, "Name") == USER_NAME
    user_token = authenticate(USER_NAME, USER_PASSWORD)

    admin_denial = assert_access_denied("GET", "/JellyPremiere/Admin/Announcements", user_token)
    library_denial = assert_access_denied("GET", f"/JellyPremiere/Library/Item/{uuid.uuid4()}", user_token)

    announcement = call("POST", "/JellyPremiere/Admin/Announcements", {
        "title": "Aviso E2E",
        "description": "Validación real Jellyfin 10.10.7",
        "type": 1,
        "isEnabled": True,
        "targetUserIds": []
    }, token=admin_token, expected=(200, 201))
    announcement_id = pick(announcement, "Id")
    assert announcement_id, announcement

    active = call("GET", "/JellyPremiere/Active", token=user_token, expected=(200,))
    assert any(pick(item, "Title") == "Aviso E2E" for item in active), active

    call("POST", f"/JellyPremiere/Acknowledge/{announcement_id}", token=user_token, expected=(200, 204))
    active_after = call("GET", "/JellyPremiere/Active", token=user_token, expected=(200,))
    assert not any(pick(item, "Id") == announcement_id for item in active_after), active_after

    me = call("GET", "/Users/Me", token=user_token, expected=(200,))
    user_id = pick(me, "Id")
    channels = call("GET", "/Channels?" + urllib.parse.urlencode({"userId": user_id}), token=user_token, expected=(200,))
    channel_items = pick(channels, "Items") or []
    estreno = next((item for item in channel_items if pick(item, "Name") == "Estrenos"), None)
    assert estreno is not None, channel_items

    print(json.dumps({
        "status": "passed",
        "announcementId": announcement_id,
        "channelId": pick(estreno, "Id"),
        "clientInjection": True,
        "adminPermissions": True,
        "adminEndpointDenialStatus": admin_denial,
        "libraryEndpointDenialStatus": library_denial,
        "normalUserAcknowledgment": True,
    }, ensure_ascii=False))


if __name__ == "__main__":
    main()
