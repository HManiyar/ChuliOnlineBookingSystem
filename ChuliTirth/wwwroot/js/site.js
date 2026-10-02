(function () {
    "use strict";

    // Mobile nav toggle
    var toggle = document.getElementById("ctNavToggle");
    var nav = document.getElementById("ctNav");
    if (toggle && nav) {
        toggle.addEventListener("click", function () {
            var open = nav.classList.toggle("open");
            toggle.setAttribute("aria-expanded", open ? "true" : "false");
        });
    }

    // Ticker pause/resume
    document.querySelectorAll(".ct-ticker-pause").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var target = document.getElementById(btn.dataset.target);
            if (!target) return;
            var paused = target.classList.toggle("paused");
            btn.setAttribute("aria-pressed", paused ? "true" : "false");
            btn.innerHTML = paused ? "&#9654;" : "&#10073;&#10073;";
        });
    });

    // Pause ticker on touch/hover so users can read it
    document.querySelectorAll(".ct-ticker-track").forEach(function (track) {
        ["mouseenter", "touchstart"].forEach(function (evt) {
            track.addEventListener(evt, function () { track.classList.add("paused"); }, { passive: true });
        });
        track.addEventListener("mouseleave", function () { track.classList.remove("paused"); });
    });

    // Sticky "Book Now" button on mobile for room detail / rooms pages
    var sticky = document.querySelector(".sticky-book-btn");
    if (sticky) {
        var showAfter = 400;
        window.addEventListener("scroll", function () {
            if (window.scrollY > showAfter) sticky.classList.add("show");
            else sticky.classList.remove("show");
        }, { passive: true });
    }

    // Simple client-side check-out >= check-in guard on booking search forms
    document.querySelectorAll("form[data-booking-search]").forEach(function (form) {
        form.addEventListener("submit", function (e) {
            var checkIn = form.querySelector('[name="checkIn"]') || form.querySelector('[name="CheckIn"]');
            var checkOut = form.querySelector('[name="checkOut"]') || form.querySelector('[name="CheckOut"]');
            if (checkIn && checkOut && checkIn.value && checkOut.value && checkOut.value <= checkIn.value) {
                e.preventDefault();
                alert("Check-out date must be after check-in date.");
            }
        });
    });

    // Picking a check-in date defaults check-out to the next day and caps how far out it can be
    // set (data-max-nights, kept in sync with ApplicationSettings.MaxBookingNights) — the server
    // is still the authoritative check, this is just so the date pickers don't let guests pick an
    // invalid range in the first place.
    function toIsoDate(d) {
        var m = String(d.getMonth() + 1).padStart(2, "0");
        var day = String(d.getDate()).padStart(2, "0");
        return d.getFullYear() + "-" + m + "-" + day;
    }
    document.querySelectorAll(".ct-checkin-input").forEach(function (checkIn) {
        var form = checkIn.closest("form");
        var checkOut = form ? form.querySelector(".ct-checkout-input") : null;
        if (!checkOut) return;
        var maxNights = parseInt(checkIn.dataset.maxNights, 10) || 3;

        function syncCheckout() {
            if (!checkIn.value) return;
            var parts = checkIn.value.split("-").map(Number);
            var checkInDate = new Date(parts[0], parts[1] - 1, parts[2]);

            var minCheckout = new Date(checkInDate);
            minCheckout.setDate(minCheckout.getDate() + 1);
            var maxCheckout = new Date(checkInDate);
            maxCheckout.setDate(maxCheckout.getDate() + maxNights);

            checkOut.min = toIsoDate(minCheckout);
            checkOut.max = toIsoDate(maxCheckout);
            if (!checkOut.value || checkOut.value < checkOut.min || checkOut.value > checkOut.max) {
                checkOut.value = toIsoDate(minCheckout);
            }
        }

        checkIn.addEventListener("change", syncCheckout);
        if (checkIn.value) syncCheckout();
    });

    // At least one adult per room — a room can't be booked by zero adults.
    document.querySelectorAll(".ct-rooms-input").forEach(function (roomsInput) {
        var form = roomsInput.closest("form");
        var adultsInput = form ? form.querySelector(".ct-adults-input") : null;
        if (!adultsInput) return;
        var errorEl = form.querySelector(".ct-adults-rooms-error");

        function validate() {
            var rooms = parseInt(roomsInput.value, 10) || 0;
            var adults = parseInt(adultsInput.value, 10) || 0;
            adultsInput.min = rooms || 1;
            var invalid = rooms > adults;
            if (errorEl) errorEl.style.display = invalid ? "block" : "none";
            return !invalid;
        }

        roomsInput.addEventListener("input", validate);
        adultsInput.addEventListener("input", validate);
        validate();

        form.addEventListener("submit", function (e) {
            if (!validate()) {
                e.preventDefault();
                alert("Number of adults must be at least the number of rooms booked — each room needs at least one adult.");
            }
        });
    });

    // Gallery lightbox — any .ct-gallery-img-link opens in an in-page viewer instead of a new
    // tab. Prev/next navigation is scoped to the other links within the same .ct-grid, so each
    // gallery category (or the homepage preview strip) browses as its own set.
    var galleryLinks = document.querySelectorAll(".ct-gallery-img-link");
    if (galleryLinks.length) {
        var overlay = document.createElement("div");
        overlay.className = "ct-lightbox-overlay";
        overlay.setAttribute("role", "dialog");
        overlay.setAttribute("aria-modal", "true");
        overlay.setAttribute("aria-label", "Image viewer");
        overlay.innerHTML =
            '<div class="ct-lightbox-figure">' +
            '<button type="button" class="ct-lightbox-close" aria-label="Close">&times;</button>' +
            '<button type="button" class="ct-lightbox-prev" aria-label="Previous image">&#8249;</button>' +
            '<img class="ct-lightbox-img" alt="" />' +
            '<button type="button" class="ct-lightbox-next" aria-label="Next image">&#8250;</button>' +
            '<div class="ct-lightbox-caption"></div>' +
            "</div>";
        document.body.appendChild(overlay);

        var imgEl = overlay.querySelector(".ct-lightbox-img");
        var captionEl = overlay.querySelector(".ct-lightbox-caption");
        var closeBtn = overlay.querySelector(".ct-lightbox-close");
        var prevBtn = overlay.querySelector(".ct-lightbox-prev");
        var nextBtn = overlay.querySelector(".ct-lightbox-next");

        var activeGroup = [];
        var activeIndex = -1;
        var lastFocused = null;

        function groupFor(link) {
            var scope = link.closest(".ct-grid") || document;
            return Array.prototype.slice.call(scope.querySelectorAll(".ct-gallery-img-link"));
        }

        function show(index) {
            if (!activeGroup.length) return;
            activeIndex = (index + activeGroup.length) % activeGroup.length;
            var link = activeGroup[activeIndex];
            var img = link.querySelector("img");
            imgEl.src = link.getAttribute("href");
            imgEl.alt = img ? img.alt : "";
            captionEl.textContent = img ? img.alt : "";
            var multi = activeGroup.length > 1;
            prevBtn.style.display = multi ? "" : "none";
            nextBtn.style.display = multi ? "" : "none";
        }

        function open(link) {
            lastFocused = document.activeElement;
            activeGroup = groupFor(link);
            show(activeGroup.indexOf(link));
            overlay.classList.add("open");
            document.body.classList.add("ct-lightbox-locked");
            closeBtn.focus();
        }

        function close() {
            overlay.classList.remove("open");
            document.body.classList.remove("ct-lightbox-locked");
            imgEl.src = "";
            if (lastFocused) lastFocused.focus();
        }

        galleryLinks.forEach(function (link) {
            link.addEventListener("click", function (e) {
                e.preventDefault();
                open(link);
            });
        });

        closeBtn.addEventListener("click", close);
        prevBtn.addEventListener("click", function () { show(activeIndex - 1); });
        nextBtn.addEventListener("click", function () { show(activeIndex + 1); });
        overlay.addEventListener("click", function (e) {
            if (e.target === overlay) close();
        });
        document.addEventListener("keydown", function (e) {
            if (!overlay.classList.contains("open")) return;
            if (e.key === "Escape") close();
            else if (e.key === "ArrowLeft") show(activeIndex - 1);
            else if (e.key === "ArrowRight") show(activeIndex + 1);
        });
    }
})();
