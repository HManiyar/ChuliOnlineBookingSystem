(function () {
    "use strict";

    var page = (location.pathname.split("/").pop() || "dashboard.html");

    // Every admin page except login.html requires the demo "auth" flag set by login.html.
    if (page !== "login.html" && !adminIsLoggedIn()) {
        location.href = "login.html";
        return;
    }

    var NAV_ITEMS = [
        { href: "dashboard.html", icon: "bi-speedometer2", label: "Dashboard" },
        { href: "bookings.html", icon: "bi-journal-check", label: "Bookings" },
        { href: "room-types.html", icon: "bi-house-door", label: "Room Types" },
        { href: "rooms.html", icon: "bi-grid-3x3-gap", label: "Room Inventory" },
        { href: "announcements.html", icon: "bi-megaphone", label: "Announcements" },
        { href: "quotes.html", icon: "bi-quote", label: "Jain Quotes" },
        { href: "tithis.html", icon: "bi-calendar3", label: "Jain Tithi" },
        { href: "bhojanshala.html", icon: "bi-egg-fried", label: "Bhojanshala" },
        { href: "facilities.html", icon: "bi-check-circle", label: "Facilities" },
        { href: "gallery.html", icon: "bi-images", label: "Gallery" },
        { href: "booking-rules.html", icon: "bi-journal-text", label: "Booking Rules" },
        { href: "contact-messages.html", icon: "bi-envelope", label: "Contact Messages" },
        { href: "settings.html", icon: "bi-gear", label: "Settings" }
    ];

    function sidebarHtml() {
        var links = NAV_ITEMS.map(function (item) {
            var activeHref = item.href === "bookings.html" && page === "booking-detail.html" ? true : item.href === page;
            return '<a class="' + (activeHref ? "active" : "") + '" href="' + item.href + '"><i class="bi ' + item.icon + '"></i> ' + item.label + "</a>";
        }).join("");
        return (
            '<aside class="ct-admin-sidebar">' +
            '<div style="padding:1.25rem;font-weight:800;color:#fff;">Chuli Tirth <span class="ct-muted" style="color:#d9c6ab;">Admin</span></div>' +
            links +
            '<div class="ct-divider" style="background:rgba(255,255,255,0.15);"></div>' +
            '<a href="../index.html"><i class="bi bi-box-arrow-left"></i> Back to Website</a>' +
            '<a href="#" id="adminLogoutLink"><i class="bi bi-door-closed"></i> Logout</a>' +
            "</aside>"
        );
    }

    function topbarHtml(title) {
        return (
            '<div class="ct-admin-topbar">' +
            "<h3 class=\"mb-0\">" + title + "</h3>" +
            '<span class="ct-muted">Demo Admin (Admin, Manager)</span>' +
            "</div>"
        );
    }

    document.addEventListener("DOMContentLoaded", function () {
        if (page === "login.html") return;

        var shellMount = document.getElementById("ct-admin-sidebar-mount");
        if (shellMount) shellMount.outerHTML = sidebarHtml();

        var topbarMount = document.getElementById("ct-admin-topbar-mount");
        if (topbarMount) topbarMount.outerHTML = topbarHtml(document.title.replace(" — Admin Preview", ""));

        var logoutLink = document.getElementById("adminLogoutLink");
        if (logoutLink) {
            logoutLink.addEventListener("click", function (e) {
                e.preventDefault();
                adminLogout();
            });
        }

        // Any element marked data-admin-action (buttons/links standing in for Confirm, Check-in,
        // Delete, Save, etc.) just shows a toast instead of mutating anything — there's no
        // backend here to mutate.
        document.addEventListener("click", function (e) {
            var el = e.target.closest("[data-admin-action]");
            if (!el) return;
            e.preventDefault();
            showAdminToast(el.getAttribute("data-admin-action") || "This is a preview — no changes are saved.");
        });
        document.addEventListener("submit", function (e) {
            if (e.target.matches("[data-admin-form]")) {
                e.preventDefault();
                showAdminToast("This is a preview — no changes are saved.");
            }
        });
    });

    var toastTimer = null;
    window.showAdminToast = function (msg) {
        var toast = document.getElementById("ct-admin-toast");
        if (!toast) {
            toast = document.createElement("div");
            toast.id = "ct-admin-toast";
            toast.className = "ct-admin-toast";
            document.body.appendChild(toast);
        }
        toast.textContent = msg;
        toast.classList.add("show");
        clearTimeout(toastTimer);
        toastTimer = setTimeout(function () { toast.classList.remove("show"); }, 2600);
    };
})();
