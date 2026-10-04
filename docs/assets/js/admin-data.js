// Admin dashboard mock data — static preview only, mirrors the real app's seeded sample data.
// Nothing here is connected to any backend; edits made in these screens do not persist.

var ADMIN_AUTH_KEY = "ct_admin_demo_auth";

function adminIsLoggedIn() {
  try { return localStorage.getItem(ADMIN_AUTH_KEY) === "1"; } catch (e) { return false; }
}
function adminLogin() {
  try { localStorage.setItem(ADMIN_AUTH_KEY, "1"); } catch (e) { /* ignore */ }
}
function adminLogout() {
  try { localStorage.removeItem(ADMIN_AUTH_KEY); } catch (e) { /* ignore */ }
  location.href = "login.html";
}
function adminRequireAuth() {
  if (!adminIsLoggedIn()) location.href = "login.html";
}

var ADMIN_BOOKINGS = [
  { id: 1, number: "CT-2026-000101", first: "Hitanshu", last: "Maniyar", mobile: "9825012345", email: "hitanshu@example.com", roomType: "AC Room", roomNumber: "204", checkIn: "2026-10-12", checkOut: "2026-10-14", status: "Confirmed", payment: "Paid", amount: 2000 },
  { id: 2, number: "CT-2026-000102", first: "Kavita", last: "Shah", mobile: "9909087654", email: "kavita.shah@example.com", roomType: "Non-AC Room", roomNumber: "7", checkIn: "2026-10-09", checkOut: "2026-10-10", status: "CheckedIn", payment: "Paid", amount: 500 },
  { id: 3, number: "CT-2026-000103", first: "Ramesh", last: "Jain", mobile: "9879011223", email: "ramesh.jain@example.com", roomType: "AC Room", roomNumber: "211", checkIn: "2026-10-05", checkOut: "2026-10-06", status: "Completed", payment: "Paid", amount: 1000 },
  { id: 4, number: "CT-2026-000104", first: "Priya", last: "Mehta", mobile: "9898076543", email: "priya.mehta@example.com", roomType: "AC Room", roomNumber: "—", checkIn: "2026-10-18", checkOut: "2026-10-20", status: "Pending", payment: "Unpaid", amount: 2000 },
  { id: 5, number: "CT-2026-000105", first: "Dilip", last: "Sanghvi", mobile: "9825098765", email: "dilip.sanghvi@example.com", roomType: "Non-AC Room", roomNumber: "12", checkIn: "2026-09-28", checkOut: "2026-09-29", status: "Cancelled", payment: "Refunded", amount: 500 },
  { id: 6, number: "CT-2026-000106", first: "Sangeeta", last: "Parikh", mobile: "9909011122", email: "sangeeta.p@example.com", roomType: "AC Room", roomNumber: "—", checkIn: "2026-10-15", checkOut: "2026-10-16", status: "PaymentPending", payment: "Unpaid", amount: 1000 },
  { id: 7, number: "CT-2026-000107", first: "Nilesh", last: "Doshi", mobile: "9879055667", email: "nilesh.doshi@example.com", roomType: "Non-AC Room", roomNumber: "203", checkIn: "2026-10-03", checkOut: "2026-10-04", status: "CheckedOut", payment: "Paid", amount: 500 },
  { id: 8, number: "CT-2026-000108", first: "Falguni", last: "Shah", mobile: "9898022334", email: "falguni.shah@example.com", roomType: "AC Room", roomNumber: "9", checkIn: "2026-10-22", checkOut: "2026-10-25", status: "Confirmed", payment: "Paid", amount: 3000 }
];

var ADMIN_CONTACT_MESSAGES = [
  { date: "2026-10-01 14:22", name: "Ramesh Jain", email: "ramesh.jain@example.com", mobile: "9879011223", subject: "Group booking for Sangh Yatra", message: "We have a group of 20 pilgrims visiting next month, is a group discount available?", read: false },
  { date: "2026-09-29 09:05", name: "Priya Mehta", email: "priya.mehta@example.com", mobile: "9898076543", subject: "Wheelchair access", message: "Does the Dharamshala have wheelchair-accessible rooms on the ground floor?", read: true },
  { date: "2026-09-25 18:40", name: "Dilip Sanghvi", email: "dilip.sanghvi@example.com", mobile: "9825098765", subject: "Bhojanshala timing on Paryushan", message: "Will Bhojanshala timings change during Paryushan Mahaparva?", read: true }
];

var ADMIN_QUOTES = [
  { en: "Ahimsa Parmo Dharma — Non-violence is the supreme religion.", gu: "અહિંસા પરમો ધર્મ", hi: "अहिंसा परमो धर्म", order: 1, active: true },
  { en: "Live and let live.", gu: "જીવો અને જીવવા દો", hi: "जियो और जीने दो", order: 2, active: true },
  { en: "Parasparopagraho Jivanam — All life is bound together by mutual support and interdependence.", gu: "પરસ્પરોપગ્રહો જીવાનામ", hi: "परस्परोपग्रहो जीवानाम्", order: 3, active: true },
  { en: "Truth is the essence of all conduct.", gu: "સત્ય એ સર્વ આચરણનો સાર છે", hi: "सत्य ही सभी आचरण का सार है", order: 4, active: true },
  { en: "Conquer anger with forgiveness.", gu: "ક્ષમાથી ક્રોધને જીતો", hi: "क्षमा से क्रोध को जीतें", order: 5, active: true }
];

var ADMIN_TITHIS = [
  { date: "2026-10-04", paksha: "Sud", tithi: "Choth", occasion: "" },
  { date: "2026-10-05", paksha: "Sud", tithi: "Pancham", occasion: "" },
  { date: "2026-10-06", paksha: "Sud", tithi: "Chhath", occasion: "" },
  { date: "2026-10-07", paksha: "Sud", tithi: "Saatam", occasion: "" },
  { date: "2026-10-24", paksha: "Vad", tithi: "Ekam", occasion: "Sample: Paryushan Mahaparva begins" }
];

var ADMIN_ANNOUNCEMENTS = [
  { title: "Welcome to Chuli Tirth Dharamshala (Sample Announcement)", priority: "Normal", active: true, window: "— ongoing —" }
];

var ADMIN_SETTINGS_LABELS = {
  DharamshalaName: "Dharamshala Name",
  Address: "Address",
  Phone: "Phone",
  Email: "Email",
  WhatsApp: "WhatsApp Number",
  GoogleMapsUrl: "Google Maps URL",
  CheckInTime: "Check-in Time",
  CheckOutTime: "Check-out Time",
  HomepageHeroText: "Homepage Hero Text",
  FooterText: "Footer Text"
};
var ADMIN_SETTINGS_VALUES = {
  DharamshalaName: "Chuli Tirth Dharamshala",
  Address: "Sample Address — Chuli Tirth, Gujarat, India",
  Phone: "+91 99999 00000",
  Email: "info@chulitirth.local",
  WhatsApp: "+91 99999 00000",
  GoogleMapsUrl: "https://maps.google.com/?q=Chuli+Jain+Tirth",
  CheckInTime: "12:00",
  CheckOutTime: "10:00",
  HomepageHeroText: "Sample: Experience peace and devotion at Chuli Tirth",
  FooterText: "Sample: Serving pilgrims with Jain hospitality and Sadharmik Bhakti."
};

function adminRoomInventory() {
  var floors = [["Ground Floor", 1, 8], ["First Floor", 9, 16], ["Second Floor", 201, 219]];
  var rooms = [];
  floors.forEach(function (f) {
    for (var n = f[1]; n <= f[2]; n++) rooms.push({ number: String(n), floor: f[0], status: "Available" });
  });
  // A few sample non-default statuses so the screen doesn't look all-identical
  var occupied = ["204", "7", "211", "12", "203", "9"];
  var maintenance = ["216"];
  rooms.forEach(function (r) {
    if (occupied.indexOf(r.number) !== -1) r.status = "Occupied";
    if (maintenance.indexOf(r.number) !== -1) r.status = "Maintenance";
  });
  return rooms;
}

function adminStats() {
  var bookings = ADMIN_BOOKINGS;
  var rooms = adminRoomInventory();
  return {
    todayCheckIns: bookings.filter(function (b) { return b.checkIn === "2026-10-04" && b.status === "Confirmed"; }).length,
    todayCheckOuts: bookings.filter(function (b) { return b.checkOut === "2026-10-04" && b.status === "CheckedIn"; }).length,
    currentGuests: 3,
    availableRooms: rooms.filter(function (r) { return r.status === "Available"; }).length,
    occupiedRooms: rooms.filter(function (r) { return r.status === "Occupied"; }).length,
    pendingBookings: bookings.filter(function (b) { return b.status === "Pending" || b.status === "PaymentPending"; }).length,
    confirmedBookings: bookings.filter(function (b) { return b.status === "Confirmed"; }).length,
    cancelledBookings: bookings.filter(function (b) { return b.status === "Cancelled"; }).length,
    revenueThisMonth: bookings.filter(function (b) { return b.payment === "Paid"; }).reduce(function (s, b) { return s + b.amount; }, 0)
  };
}
