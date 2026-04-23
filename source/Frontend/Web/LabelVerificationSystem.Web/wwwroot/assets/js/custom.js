(function () {
  "use strict";


 document.addEventListener("DOMContentLoaded", function () {
    // Ensure Waves is defined before using it
    if (typeof Waves !== "undefined") {
      // node waves
      Waves.attach(".btn-wave", ["waves-light"]);
      Waves.init();
      // node waves
    } else {
      console.error("Waves is not defined. Make sure waves.min.js is loaded before custom.js.");
    }
  });


  window.addEventListener('scroll', () => {
    var windowScroll = document.body.scrollTop || document.documentElement.scrollTop,
        height = document.documentElement.scrollHeight - document.documentElement.clientHeight,
        scrollAmount = (windowScroll / height) * 100;

    var progressBar = document.querySelector(".progress-top-bar");
    if (progressBar) {
      progressBar.style.width = scrollAmount + "%";
    }
  });


})();

/* toggle switches */
let customSwitch = document.querySelectorAll(".toggle");
customSwitch.forEach((e) =>
  e.addEventListener("click", () => {
    e.classList.toggle("on");
  })
);
/* toggle switches */

setTimeout(() => {
    const inputElem = document.querySelector("#header-search");
    if (!inputElem) {
        return;
    }

    const autoCompleteJS = new autoComplete({
        selector: "#header-search",
        data: {
            src: [
                "How do plants adapt to different environments?",
                "What makes the ocean's tides rise and fall?",
                "How do our brains process emotions?",
                "What factors contribute to the creation of a rainbow?",
                "Who invented the telephone?",
                "What role does the moon play in Earth's ecosystem?",
                "How do animals communicate with each other?",
                "What causes earthquakes to happen?",
                "What is the significance of the Great Barrier Reef?",
                "How do human bones regenerate after an injury?"
            ],
            cache: true,
        },
        resultItem: {
            highlight: true
        },
        events: {
            input: {
                selection: (event) => {
                    const selection = event.detail.selection.value;
                    autoCompleteJS.input.value = selection;
                }
            }
        }
    });
}, 300);


const scrollToTop = document.querySelector(".scrollToTop");
const $rootElement = document.documentElement;
const $body = document.body;

if (scrollToTop) {
  window.onscroll = () => {
    const scrollTop = window.scrollY || window.pageYOffset;
    const clientHt = $rootElement.scrollHeight - $rootElement.clientHeight;
    if (scrollTop > 100) {
      scrollToTop.style.display = "flex";
    } else {
      scrollToTop.style.display = "none";
    }
  };

  scrollToTop.onclick = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  };
}

/* count-up */
var i = 1;
setInterval(() => {
  document.querySelectorAll(".count-up").forEach((ele) => {
    if (ele.getAttribute("data-count") >= i) {
      i = i + 1;
      ele.innerText = i;
    }
  });
}, 10);
/* count-up */
