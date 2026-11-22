// Wizytówka: light/dark toggle, language search, closing the dropdowns. Loaded in <head> (the saved theme applies
// before paint); the rest waits for DOMContentLoaded. Without this script the page still works: system theme,
// dropdowns open and close on click (<details>), the language list is shown in full.
(function () {
  var KEY = 'aed-theme';
  var root = document.documentElement;

  function read() {
    try {
      return localStorage.getItem(KEY);
    } catch (e) {
      return null;
    }
  }

  function save(value) {
    try {
      localStorage.setItem(KEY, value);
    } catch (e) {
      /* private mode: the choice lasts until the page is closed */
    }
  }

  function currentTheme() {
    var chosen = root.getAttribute('data-theme');
    if (chosen === 'light' || chosen === 'dark') {
      return chosen;
    }
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  var saved = read();
  if (saved === 'light' || saved === 'dark') {
    root.setAttribute('data-theme', saved);
  }

  function setupTheme() {
    var button = document.querySelector('.theme-toggle');
    if (!button) {
      return;
    }
    function sync() {
      button.setAttribute('aria-pressed', currentTheme() === 'dark' ? 'true' : 'false');
    }
    button.hidden = false;
    sync();
    button.addEventListener('click', function () {
      var next = currentTheme() === 'dark' ? 'light' : 'dark';
      root.setAttribute('data-theme', next);
      save(next);
      sync();
    });
  }

  function setupLanguageSearch() {
    var menu = document.querySelector('.lang');
    if (!menu) {
      return;
    }
    var box = menu.querySelector('.lang-search');
    var input = box && box.querySelector('input');
    var items = menu.querySelectorAll('.lang-list li');
    var empty = menu.querySelector('.lang-empty');
    if (!input) {
      return;
    }
    box.hidden = false;

    function filter() {
      var query = input.value.trim().toLowerCase();
      var shown = 0;
      Array.prototype.forEach.call(items, function (item) {
        var match = !query || (item.getAttribute('data-name') || '').indexOf(query) !== -1;
        item.hidden = !match;
        if (match) {
          shown += 1;
        }
      });
      if (empty) {
        empty.hidden = shown !== 0;
      }
    }

    input.addEventListener('input', filter);
    // Enter opens the first visible language.
    input.addEventListener('keydown', function (event) {
      if (event.key === 'Enter') {
        var first = menu.querySelector('.lang-list li:not([hidden]) a');
        if (first) {
          event.preventDefault();
          first.click();
        }
      }
    });
    menu.addEventListener('toggle', function () {
      if (menu.open) {
        input.value = '';
        filter();
        input.focus();
      }
    });
  }

  // Only one dropdown open; Escape and a click outside close it.
  function setupDropdowns() {
    var dropdowns = document.querySelectorAll('details.dropdown');
    Array.prototype.forEach.call(dropdowns, function (d) {
      d.addEventListener('toggle', function () {
        if (d.open) {
          Array.prototype.forEach.call(dropdowns, function (other) {
            if (other !== d) {
              other.open = false;
            }
          });
        }
      });
    });
    document.addEventListener('click', function (event) {
      Array.prototype.forEach.call(dropdowns, function (d) {
        if (d.open && !d.contains(event.target)) {
          d.open = false;
        }
      });
    });
    document.addEventListener('keydown', function (event) {
      if (event.key !== 'Escape') {
        return;
      }
      Array.prototype.forEach.call(dropdowns, function (d) {
        if (d.open) {
          d.open = false;
          var summary = d.querySelector('summary');
          if (summary) {
            summary.focus();
          }
        }
      });
    });
  }

  // Links to the app point at production (https://aed.hacknation-2026.pl/<lang>). On this computer they go to the
  // local app instead: localhost -> the Aspire dev server (port 3060), *.test -> the local Traefik (deploy/edge/local).
  var APP_URL = 'https://aed.hacknation-2026.pl';

  function localAppUrl() {
    var host = location.hostname;
    if (host === 'localhost' || host === '127.0.0.1') {
      return 'http://localhost:3060';
    }
    if (/\.test$/.test(host)) {
      return location.protocol + '//aed.hacknation-2026.test' + (location.port ? ':' + location.port : '');
    }
    return null;
  }

  function setupAppLinks() {
    var local = localAppUrl();
    if (!local) {
      return;
    }
    var links = document.querySelectorAll('a[href^="' + APP_URL + '"]');
    Array.prototype.forEach.call(links, function (a) {
      a.setAttribute('href', local + a.getAttribute('href').slice(APP_URL.length));
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    setupAppLinks();
    setupTheme();
    setupLanguageSearch();
    setupDropdowns();
  });
})();
