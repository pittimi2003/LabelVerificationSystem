(function () {
    "use strict";

    const markers = [
        { name: 'Argentina', coords: [-38.4161, -63.6167] },
        { name: 'France', coords: [46.6034, 1.8883] },
        { name: 'USA', coords: [37.0902, -95.7129] }
    ]
    const map = new jsVectorMap({
        selector: "#sales-locations",
        // -------- Labels --------
        labels: {
            markers: {
                render: function (marker) {
                    return marker.name
                },
                offsets: function (index) {
                    return markers[index].offsets || [0, 0]
                }
            },
            color: "#000",
        },
        map: "world_merc",
        markers: markers,
        zoomOnScroll: false,
        zoomButtons: false,
        markerStyle: {
            initial: {
                r: 5,
                fill: 'var(--primary-color)',
                stroke: 'rgba(255,255,255,0.1)',
                strokeWidth: 2,
            }
        },
        markerLabelStyle: {
            initial: {
                fontSize: 13,
                fontWeight: 500,
                fill: '#35373e',
            },
        },
    });

})();