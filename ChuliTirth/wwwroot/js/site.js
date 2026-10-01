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
})();
