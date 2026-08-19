(function () {
    'use strict';

    console.log('[JellyPremiere] Client initialized');

    let processedAnnouncements = new Set();

    function getApiUrl(path) {
        if (window.ApiClient && typeof window.ApiClient.getUrl === 'function') {
            return window.ApiClient.getUrl(path);
        }
        return path;
    }

    function checkActiveAnnouncements() {
        if (!window.ApiClient || !window.ApiClient.getCurrentUserId) {
            return;
        }

        const userId = window.ApiClient.getCurrentUserId();
        if (!userId) {
            return;
        }

        window.ApiClient.ajax({
            type: 'GET',
            url: getApiUrl('JellyPremiere/Active'),
            dataType: 'json'
        }).then(announcements => {
            if (!announcements || !announcements.length) {
                return;
            }

            announcements.forEach(a => {
                if (processedAnnouncements.has(a.id)) {
                    return;
                }

                if (a.type === 0) {
                    // Type 0: Banner
                    renderBanner(a);
                    processedAnnouncements.add(a.id);
                } else if (a.type === 1 || a.type === 2) {
                    // Type 1: Important Notice, Type 2: Mandatory Notice
                    renderNoticeModal(a);
                    processedAnnouncements.add(a.id);
                }
            });
        }).catch(err => {
            console.warn('[JellyPremiere] Failed to fetch active announcements', err);
        });
    }

    function renderBanner(announcement) {
        // Find home view container
        const page = document.querySelector('.page:not(.hide)');
        if (!page) return;

        // Ensure we don't duplicate the banner
        if (document.getElementById('jp-banner-' + announcement.id)) {
            return;
        }

        const bannerContainer = document.createElement('div');
        bannerContainer.id = 'jp-banner-' + announcement.id;
        bannerContainer.className = 'jp-banner-card';

        let backdropUrl = announcement.mediaMetadata?.backdropUrl;
        if (backdropUrl && !backdropUrl.startsWith('http')) {
            backdropUrl = getApiUrl(backdropUrl);
        }

        const bgStyle = backdropUrl ? `background-image: url('${backdropUrl}');` : 'background: linear-gradient(135deg, #1f2937, #111827);';

        bannerContainer.style.cssText = `
            position: relative;
            margin: 20px auto;
            max-width: 1200px;
            width: 95%;
            border-radius: 12px;
            overflow: hidden;
            min-height: 220px;
            background-size: cover;
            background-position: center;
            box-shadow: 0 8px 24px rgba(0, 0, 0, 0.6);
            display: flex;
            align-items: flex-end;
            padding: 24px;
            box-sizing: border-box;
            border: 1px solid rgba(255, 255, 255, 0.1);
            ${bgStyle}
        `;

        const overlay = document.createElement('div');
        overlay.style.cssText = `
            position: absolute;
            top: 0; left: 0; right: 0; bottom: 0;
            background: linear-gradient(to top, rgba(0, 0, 0, 0.95) 30%, rgba(0, 0, 0, 0.3) 100%);
            z-index: 1;
        `;

        const content = document.createElement('div');
        content.style.cssText = `
            position: relative;
            z-index: 2;
            color: #fff;
            max-width: 800px;
        `;

        const tag = document.createElement('span');
        tag.innerText = 'ESTRENO / ANUNCIO';
        tag.style.cssText = `
            background: #00a4dc;
            color: #fff;
            font-size: 0.75rem;
            font-weight: 700;
            padding: 3px 8px;
            border-radius: 4px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            display: inline-block;
            margin-bottom: 8px;
        `;

        const title = document.createElement('h2');
        title.innerText = announcement.title;
        title.style.cssText = `
            margin: 0 0 8px 0;
            font-size: 1.8rem;
            font-weight: 700;
            line-height: 1.2;
            color: #ffffff;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.8);
        `;

        const desc = document.createElement('p');
        desc.innerText = announcement.description;
        desc.style.cssText = `
            margin: 0 0 16px 0;
            font-size: 1rem;
            line-height: 1.4;
            color: #e0e0e0;
            text-shadow: 0 1px 2px rgba(0, 0, 0, 0.8);
        `;

        content.appendChild(tag);
        content.appendChild(title);
        content.appendChild(desc);

        if (announcement.buttonText || announcement.actionUrl || announcement.libraryItemId) {
            const btn = document.createElement('button');
            btn.className = 'raised button-submit block emby-button';
            btn.innerText = announcement.buttonText || 'VER INFORMACIÓN';
            btn.style.cssText = `
                background-color: #00a4dc;
                color: #fff;
                border: none;
                padding: 10px 20px;
                border-radius: 6px;
                font-weight: 600;
                cursor: pointer;
                font-size: 0.95rem;
            `;

            btn.addEventListener('click', function () {
                if (announcement.actionUrl) {
                    window.location.hash = announcement.actionUrl;
                } else if (announcement.libraryItemId) {
                    window.location.hash = '#/details?id=' + announcement.libraryItemId;
                }
            });

            content.appendChild(btn);
        }

        bannerContainer.appendChild(overlay);
        bannerContainer.appendChild(content);

        // Inject banner into top of home page view
        const targetSection = page.querySelector('.sections') || page.querySelector('.contentScrollSlider') || page;
        if (targetSection.firstChild) {
            targetSection.insertBefore(bannerContainer, targetSection.firstChild);
        } else {
            targetSection.appendChild(bannerContainer);
        }
    }

    function renderNoticeModal(announcement) {
        // Prevent duplicate modal
        if (document.getElementById('jp-modal-' + announcement.id)) {
            return;
        }

        const isMandatory = announcement.type === 2;

        const modalOverlay = document.createElement('div');
        modalOverlay.id = 'jp-modal-' + announcement.id;
        modalOverlay.style.cssText = `
            position: fixed;
            top: 0; left: 0; right: 0; bottom: 0;
            background: rgba(0, 0, 0, 0.85);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 999999;
            padding: 20px;
            box-sizing: border-box;
            backdrop-filter: blur(5px);
        `;

        const modalContent = document.createElement('div');
        modalContent.style.cssText = `
            background: #181818;
            border-radius: 12px;
            padding: 28px;
            max-width: 550px;
            width: 100%;
            box-shadow: 0 12px 32px rgba(0, 0, 0, 0.8);
            border: 1px solid #333;
            color: #fff;
            box-sizing: border-box;
            font-family: inherit;
        `;

        const badge = document.createElement('span');
        badge.innerText = isMandatory ? 'AVISO OBLIGATORIO' : 'AVISO IMPORTANTE';
        badge.style.cssText = `
            background: ${isMandatory ? '#e50914' : '#f57c00'};
            color: #fff;
            font-size: 0.75rem;
            font-weight: 700;
            padding: 3px 8px;
            border-radius: 4px;
            text-transform: uppercase;
            display: inline-block;
            margin-bottom: 12px;
        `;

        const title = document.createElement('h2');
        title.innerText = announcement.title;
        title.style.cssText = `
            margin: 0 0 12px 0;
            font-size: 1.5rem;
            font-weight: 700;
            color: #fff;
        `;

        const desc = document.createElement('p');
        desc.innerText = announcement.description;
        desc.style.cssText = `
            margin: 0 0 24px 0;
            font-size: 1rem;
            line-height: 1.5;
            color: #ccc;
            white-space: pre-wrap;
        `;

        const actions = document.createElement('div');
        actions.style.cssText = `
            display: flex;
            justify-content: flex-end;
            gap: 12px;
        `;

        if (announcement.actionUrl || announcement.libraryItemId) {
            const actionBtn = document.createElement('button');
            actionBtn.className = 'raised button-submit emby-button';
            actionBtn.innerText = announcement.buttonText || 'VER INFORMACIÓN';
            actionBtn.style.cssText = `
                background-color: #00a4dc;
                color: #fff;
                border: none;
                padding: 10px 18px;
                border-radius: 6px;
                font-weight: 600;
                cursor: pointer;
            `;
            actionBtn.addEventListener('click', function () {
                if (announcement.actionUrl) {
                    window.location.hash = announcement.actionUrl;
                } else if (announcement.libraryItemId) {
                    window.location.hash = '#/details?id=' + announcement.libraryItemId;
                }
            });
            actions.appendChild(actionBtn);
        }

        const ackBtn = document.createElement('button');
        ackBtn.className = 'raised button-submit emby-button';
        ackBtn.innerText = isMandatory ? 'ENTENDIDO' : 'CERRAR';
        ackBtn.style.cssText = `
            background-color: ${isMandatory ? '#e50914' : '#444'};
            color: #fff;
            border: none;
            padding: 10px 22px;
            border-radius: 6px;
            font-weight: 700;
            cursor: pointer;
        `;

        ackBtn.addEventListener('click', function () {
            // Send acknowledgment request to server
            if (window.ApiClient) {
                window.ApiClient.ajax({
                    type: 'POST',
                    url: getApiUrl('JellyPremiere/Acknowledge/' + announcement.id)
                }).then(() => {
                    modalOverlay.remove();
                }).catch(err => {
                    console.error('[JellyPremiere] Acknowledgment error', err);
                    modalOverlay.remove();
                });
            } else {
                modalOverlay.remove();
            }
        });

        actions.appendChild(ackBtn);

        modalContent.appendChild(badge);
        modalContent.appendChild(title);
        modalContent.appendChild(desc);
        modalContent.appendChild(actions);
        modalOverlay.appendChild(modalContent);

        document.body.appendChild(modalOverlay);

        // Support focus for TV / Remote controllers
        ackBtn.focus();
    }

    // Periodically check for active announcements & on page navigate
    setInterval(checkActiveAnnouncements, 5000);
    window.addEventListener('hashchange', function () {
        setTimeout(checkActiveAnnouncements, 800);
    });

    if (document.readyState === 'complete' || document.readyState === 'interactive') {
        setTimeout(checkActiveAnnouncements, 1000);
    } else {
        document.addEventListener('DOMContentLoaded', function () {
            setTimeout(checkActiveAnnouncements, 1000);
        });
    }

})();
