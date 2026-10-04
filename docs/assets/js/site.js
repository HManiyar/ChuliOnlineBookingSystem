(function () {
    "use strict";

    var LANG_KEY = "ct_preview_lang";

    function getLang() {
        return localStorage.getItem(LANG_KEY) || "en";
    }

    function setLang(lang) {
        try { localStorage.setItem(LANG_KEY, lang); } catch (e) { /* private browsing etc. */ }
        applyLang();
    }
    window.setLang = setLang;
    window.getLang = getLang;

    function navbarHtml(lang) {
        var page = (location.pathname.split("/").pop() || "index.html");
        function link(href, key) {
            var active = page === href ? " active" : "";
            return '<a class="' + active.trim() + '" href="' + href + '">' + t(key, lang) + "</a>";
        }
        function langLink(code, label) {
            var active = lang === code ? " active" : "";
            return '<a class="' + active.trim() + '" href="#" data-lang="' + code + '">' + label + "</a>";
        }
        return (
            '<header class="ct-header">' +
            '<div class="container-ct ct-header-inner">' +
            '<a href="index.html" class="ct-brand"><span class="emblem"><i class="bi bi-flower1"></i></span><span>' + SITE_SETTINGS.DharamshalaName + "</span></a>" +
            '<button class="ct-hamburger" type="button" id="ctNavToggle" aria-label="Toggle navigation" aria-expanded="false"><i class="bi bi-list"></i></button>' +
            '<nav class="ct-nav" id="ctNav">' +
            link("index.html", "Home") +
            link("about.html", "About") +
            link("rooms.html", "Rooms") +
            link("facilities.html", "Facilities") +
            link("bhojanshala.html", "Bhojanshala") +
            link("gallery.html", "Gallery") +
            link("booking-rules.html", "BookingRules") +
            link("contact.html", "Contact") +
            link("how-to-reach.html", "HowToReach") +
            "</nav>" +
            '<div class="ct-header-actions">' +
            '<div class="ct-lang-switch">' +
            langLink("gu", "ગુજરાતી") + langLink("hi", "हिन्दी") + langLink("en", "English") +
            "</div>" +
            '<a href="rooms.html" class="btn-ct-primary"><i class="bi bi-calendar-check"></i> ' + t("BookNow", lang) + "</a>" +
            "</div>" +
            "</div>" +
            "</header>"
        );
    }

    function footerHtml(lang) {
        return (
            '<footer class="ct-footer">' +
            '<div class="container-ct ct-grid" style="grid-template-columns: repeat(auto-fit, minmax(200px,1fr));">' +
            "<div><h5>" + SITE_SETTINGS.DharamshalaName + "</h5><p>Serving pilgrims with Jain hospitality and Sadharmik Bhakti.</p></div>" +
            "<div><h5>" + t("QuickLinks", lang) + '</h5><p><a href="rooms.html">' + t("RoomBooking", lang) + '</a></p><p><a href="facilities.html">' + t("Facilities", lang) + '</a></p><p><a href="contact.html">' + t("Contact", lang) + "</a></p></div>" +
            "<div><h5>" + t("Policies", lang) + '</h5><p><a href="booking-rules.html">' + t("BookingRules", lang) + '</a></p><p><a href="how-to-reach.html">' + t("HowToReach", lang) + "</a></p></div>" +
            "<div><h5>" + t("ContactHeading", lang) + '</h5><p><i class="bi bi-geo-alt"></i> ' + SITE_SETTINGS.Address + '</p><p><i class="bi bi-telephone"></i> ' + SITE_SETTINGS.Phone + '</p><p><i class="bi bi-envelope"></i> ' + SITE_SETTINGS.Email + '</p><p><a href="' + SITE_SETTINGS.GoogleMapsUrl + '" target="_blank" rel="noopener">' + t("ViewOnMaps", lang) + "</a></p></div>" +
            "</div>" +
            '<div class="container-ct bottom">&copy; ' + new Date().getFullYear() + " " + SITE_SETTINGS.DharamshalaName + ". " + t("AllRightsReserved", lang) + ' <span class="ct-muted">' + t("SampleContentFooterNote", lang) + "</span></div>" +
            "</footer>"
        );
    }

    function applyLang() {
        var lang = getLang();
        document.documentElement.lang = lang;

        var navMount = document.getElementById("ct-navbar-mount");
        var footMount = document.getElementById("ct-footer-mount");
        if (navMount) navMount.innerHTML = navbarHtml(lang);
        if (footMount) footMount.innerHTML = footerHtml(lang);

        document.querySelectorAll("[data-i18n]").forEach(function (el) {
            el.textContent = t(el.getAttribute("data-i18n"), lang);
        });
        document.querySelectorAll("[data-i18n-html]").forEach(function (el) {
            el.innerHTML = t(el.getAttribute("data-i18n-html"), lang);
        });

        if (navMount) {
            navMount.querySelectorAll("[data-lang]").forEach(function (a) {
                a.addEventListener("click", function (e) {
                    e.preventDefault();
                    setLang(a.getAttribute("data-lang"));
                });
            });
            var toggle = document.getElementById("ctNavToggle");
            var nav = document.getElementById("ctNav");
            if (toggle && nav) {
                toggle.addEventListener("click", function () {
                    var open = nav.classList.toggle("open");
                    toggle.setAttribute("aria-expanded", open ? "true" : "false");
                });
            }
        }

        if (window.renderDynamicContent) window.renderDynamicContent(lang);
        initLightbox();
    }

    // Gallery lightbox — re-initialized after every language switch / dynamic render, since
    // those can replace the gallery links in the DOM.
    var lightboxBuilt = false;
    function initLightbox() {
        var galleryLinks = document.querySelectorAll(".ct-gallery-img-link");
        if (!galleryLinks.length) return;

        var overlay = document.getElementById("ct-lightbox-overlay");
        if (!overlay) {
            overlay = document.createElement("div");
            overlay.id = "ct-lightbox-overlay";
            overlay.className = "ct-lightbox-overlay";
            overlay.setAttribute("role", "dialog");
            overlay.setAttribute("aria-modal", "true");
            overlay.innerHTML =
                '<div class="ct-lightbox-figure">' +
                '<button type="button" class="ct-lightbox-close" aria-label="Close">&times;</button>' +
                '<button type="button" class="ct-lightbox-prev" aria-label="Previous image">&#8249;</button>' +
                '<img class="ct-lightbox-img" alt="" />' +
                '<button type="button" class="ct-lightbox-next" aria-label="Next image">&#8250;</button>' +
                '<div class="ct-lightbox-caption"></div>' +
                "</div>";
            document.body.appendChild(overlay);
        }

        var imgEl = overlay.querySelector(".ct-lightbox-img");
        var captionEl = overlay.querySelector(".ct-lightbox-caption");
        var closeBtn = overlay.querySelector(".ct-lightbox-close");
        var prevBtn = overlay.querySelector(".ct-lightbox-prev");
        var nextBtn = overlay.querySelector(".ct-lightbox-next");

        var activeGroup = [];
        var activeIndex = -1;

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
        }

        galleryLinks.forEach(function (link) {
            link.replaceWith(link.cloneNode(true)); // drop any stale listeners from a prior render
        });
        document.querySelectorAll(".ct-gallery-img-link").forEach(function (link) {
            link.addEventListener("click", function (e) {
                e.preventDefault();
                open(link);
            });
        });

        if (!lightboxBuilt) {
            closeBtn.addEventListener("click", close);
            prevBtn.addEventListener("click", function () { show(activeIndex - 1); });
            nextBtn.addEventListener("click", function () { show(activeIndex + 1); });
            overlay.addEventListener("click", function (e) { if (e.target === overlay) close(); });
            document.addEventListener("keydown", function (e) {
                if (!overlay.classList.contains("open")) return;
                if (e.key === "Escape") close();
                else if (e.key === "ArrowLeft") show(activeIndex - 1);
                else if (e.key === "ArrowRight") show(activeIndex + 1);
            });
            lightboxBuilt = true;
        }
    }

    // Preview-only booking/search forms: never submit anywhere, just show a friendly note.
    document.addEventListener("submit", function (e) {
        if (e.target.matches("[data-preview-form]")) {
            e.preventDefault();
            var note = e.target.querySelector(".ct-preview-form-note");
            if (note) { note.style.display = "block"; note.scrollIntoView({ behavior: "smooth", block: "center" }); }
        }
    });

    // Check-in date picker defaults check-out to the next day (visual only, same UX as the real
    // app's date pickers, just without a server round-trip).
    function toIsoDate(d) {
        var m = String(d.getMonth() + 1).padStart(2, "0");
        var day = String(d.getDate()).padStart(2, "0");
        return d.getFullYear() + "-" + m + "-" + day;
    }
    document.addEventListener("change", function (e) {
        if (!e.target.matches(".ct-checkin-input")) return;
        var checkIn = e.target;
        var form = checkIn.closest("form");
        var checkOut = form ? form.querySelector(".ct-checkout-input") : null;
        if (!checkOut || !checkIn.value) return;
        var maxNights = parseInt(checkIn.dataset.maxNights, 10) || 3;
        var parts = checkIn.value.split("-").map(Number);
        var checkInDate = new Date(parts[0], parts[1] - 1, parts[2]);
        var minCheckout = new Date(checkInDate); minCheckout.setDate(minCheckout.getDate() + 1);
        var maxCheckout = new Date(checkInDate); maxCheckout.setDate(maxCheckout.getDate() + maxNights);
        checkOut.min = toIsoDate(minCheckout);
        checkOut.max = toIsoDate(maxCheckout);
        checkOut.value = toIsoDate(minCheckout);
    });

    document.addEventListener("DOMContentLoaded", applyLang);
})();
