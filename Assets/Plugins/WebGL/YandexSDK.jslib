mergeInto(LibraryManager.library, {
    GameReadyJS: function () {
        if (window.ysdk) {
            window.ysdk.features.LoadingAPI.ready();
        }
    },
    InitEventsJS: function () {
        if (window.ysdk) {
            ysdk.on('game_api_pause', () => {
                SendMessage('GameManager', 'OnAdOpen');
            });
            ysdk.on('game_api_resume', () => {
                SendMessage('OnAdClose');
                SendMessage('GameManager', 'OnAdClose');
            });
        }
    },
    ShowFullscreenAdJS: function () {
        window.ysdk.adv.showFullscreenAdv({
            callbacks: {
                onOpen: () => {
                    SendMessage('GameManager', 'OnAdOpen');
                },
                onClose: (wasShown) => {
                    SendMessage('GameManager', 'OnAdClose');
                },
                onError: (error) => {
                    SendMessage('GameManager', 'OnAdClose');
                }
            }
        });
    },

});