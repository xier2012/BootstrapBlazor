(function () {
    window.location = {
        getLocation: function (el) {
            console.log('start getLocation');
            var $el = $(el);
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(showPosition(el, position));
            } else {
                el.innerHTML = "Geolocation is not supported by this browser.";
            }
        },
        showPosition: function (el, position) {
            el.innerHTML = "Latitude: " + position.coords.latitude +
                "<br>Longitude: " + position.coords.longitude;
        }
    };
})();
