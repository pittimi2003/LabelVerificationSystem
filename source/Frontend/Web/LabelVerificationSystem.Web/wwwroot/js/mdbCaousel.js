
function initMDBCarousel() {
    // Optional: Log or force MDB init if needed
    if (typeof mdb !== "undefined") {
        // If needed: new mdb.Carousel(document.getElementById('carouselExampleIndicators'));
        console.log("MDB Carousel initialized");
    } else {
        console.warn("MDB is not loaded.");
    }
}
