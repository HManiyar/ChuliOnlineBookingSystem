# Static preview build (`conversion-to-html` branch)

This `docs/` folder is a **static, database-free HTML/CSS/JS snapshot** of the Chuli Tirth
Dharamshala site's public pages, built for sharing a quick visual preview (e.g. via GitHub
Pages) without needing the .NET app, MySQL, or any backend running.

It is **not** the production app — see the rest of this branch (and `main`) for that. This is
a temporary, standalone mirror meant to show "how the site looks."

## What's here vs. what's not

- All page content (room prices, descriptions, facilities, Bhojanshala timings, booking rules,
  gallery) is hand-copied from the real app's seed data (`DataSeeder.cs`) directly into
  `assets/js/data.js` — there is no live data source, so changes made in the real app's Admin
  panel will **not** appear here automatically.
- The English/Gujarati/Hindi language switcher is fully functional — it's a client-side port of
  the real app's `UiText.cs` dictionary, swapping text via JavaScript instead of a server
  round-trip.
- Booking/search/contact forms are present for layout purposes but **do not submit anywhere** —
  submitting shows an inline "preview only" note instead.
- Login, Admin, My Bookings, and the Jain Tithi calendar are not included (no backend to support
  them in a static preview).
- Terms & Privacy pages are not included (legal content pending Trust/legal review in the real
  app).

## Running it locally

Any static file server works, e.g. from this `docs/` folder:

```
npx http-server -p 8080
```

Then open `http://localhost:8080/`.

## Publishing on GitHub Pages

1. Push this branch to GitHub (already done if you're reading this there).
2. In the repo: **Settings → Pages → Build and deployment → Source: Deploy from a branch**.
3. Branch: `conversion-to-html`, folder: **`/docs`**. Save.
4. GitHub will publish at `https://<your-username>.github.io/<repo-name>/` within a minute or
   two.

A `.nojekyll` file is included so GitHub Pages serves the files as-is without Jekyll processing.
