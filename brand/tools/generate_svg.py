"""Generates the vector sources of the AED Radar logo (brand/src/*.svg).

The wordmark and the slogan are converted to outlines from Inter (SIL Open Font License 1.1), so the SVG files
do not depend on fonts installed on the viewer's computer.

Usage (fontTools and Inter WOFF files from @fontsource/inter 5.0.18 are needed only to regenerate):
    python -m pip install fonttools
    python brand/tools/generate_svg.py <folder with inter-latin-800.woff, inter-latin-ext-800.woff,
                                        inter-latin-600.woff, inter-latin-ext-600.woff>
"""
import sys
from pathlib import Path

from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.pens.transformPen import TransformPen
from fontTools.ttLib import TTFont

# Colours of the brand book (also the site tokens in site/public/styles.css).
GREEN = "#00843A"   # AED sign green (ISO 7010 E010); white on it 4.8:1
RED = "#C8102E"     # heart
NAVY = "#0B1F3A"    # text; 16.5:1 on white
GREY = "#3D4F6A"    # slogan; 8.3:1 on white

OUT = Path(__file__).resolve().parent.parent / "src"


class Font:
    """Glyph outlines from the Latin subset, with Latin Extended (Polish letters) as the fallback."""

    def __init__(self, *files: Path):
        self.fonts = [TTFont(str(f)) for f in files]

    def _find(self, ch: str):
        for f in self.fonts:
            name = f.getBestCmap().get(ord(ch))
            if name:
                return f, name
        raise KeyError(ch)

    def width(self, text: str, size: float, tracking: float = 0) -> float:
        total = 0.0
        for ch in text:
            f, name = self._find(ch)
            total += f["hmtx"][name][0] * size / f["head"].unitsPerEm + tracking
        return total - tracking

    def path(self, text: str, x: float, baseline: float, size: float, tracking: float = 0) -> str:
        """SVG path data of the text with its baseline at y and left edge at x."""
        parts = []
        for ch in text:
            f, name = self._find(ch)
            scale = size / f["head"].unitsPerEm
            glyphs = f.getGlyphSet()
            pen = SVGPathPen(glyphs)
            # Font units have y up; SVG has y down.
            glyphs[name].draw(TransformPen(pen, (scale, 0, 0, -scale, x, baseline)))
            d = pen.getCommands()
            if d:
                parts.append(d)
            x += f["hmtx"][name][0] * scale + tracking
        return " ".join(parts)


# --- Sygnet on a 64 x 64 grid -----------------------------------------------------------------------------------
# Two radar arcs (radius 7.5 and 14.5, 100 degrees) centred at (32, 28), the top of the heart, so the signal goes
# out of the heart; red dot = the device on the map. The drawing spans y 11.7-52, optically centred in the tile.
ARC_INNER = "M26.25 23.18A7.5 7.5 0 0 1 37.75 23.18"
ARC_OUTER = "M20.89 18.68A14.5 14.5 0 0 1 43.11 18.68"
HEART = ("M32 52C32 52 17 43 17 34.5C17 30.6 20 28 23.6 28C27 28 29.8 29.9 32 33"
         "C34.2 29.9 37 28 40.4 28C44 28 47 30.6 47 34.5C47 43 32 52 32 52Z")
DOT = (32, 38.5, 3.4)
STROKE = 3.6


def mark(tile: str | None, fg: str, dot: str, outer_opacity: float = 0.6, outer: bool = True) -> str:
    """The sygnet as SVG elements in the 64 x 64 box."""
    arcs = f'<path d="{ARC_INNER}" fill="none" stroke="{fg}" stroke-width="{STROKE}" stroke-linecap="round"/>'
    if outer:
        arcs = (f'<path d="{ARC_OUTER}" fill="none" stroke="{fg}" stroke-width="{STROKE}" stroke-linecap="round" '
                f'opacity="{outer_opacity}"/>' + arcs)
    bg = f'<rect width="64" height="64" rx="14" fill="{tile}"/>' if tile else ""
    cx, cy, r = DOT
    return f'{bg}{arcs}<path d="{HEART}" fill="{fg}"/><circle cx="{cx}" cy="{cy}" r="{r}" fill="{dot}"/>'


def mark_mono(colour: str, mask_id: str) -> str:
    """Single-colour sygnet: the tile with the arcs and heart cut out (the background shows through)."""
    cx, cy, r = DOT
    return (f'<defs><mask id="{mask_id}" maskUnits="userSpaceOnUse" x="0" y="0" width="64" height="64">'
            f'<rect width="64" height="64" fill="#fff"/>'
            f'<path d="{ARC_OUTER}" fill="none" stroke="#000" stroke-width="{STROKE}" stroke-linecap="round"/>'
            f'<path d="{ARC_INNER}" fill="none" stroke="#000" stroke-width="{STROKE}" stroke-linecap="round"/>'
            f'<path d="{HEART}" fill="#000"/><circle cx="{cx}" cy="{cy}" r="{r}" fill="#fff"/></mask></defs>'
            f'<rect width="64" height="64" rx="14" fill="{colour}" mask="url(#{mask_id})"/>')


def svg(width: float, height: float, title: str, body: str) -> str:
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {width:.1f} {height:.1f}" '
            f'width="{width:.1f}" height="{height:.1f}" role="img" aria-labelledby="t">\n'
            f'  <title id="t">{title}</title>\n  {body}\n</svg>\n')


def main(font_dir: Path) -> None:
    bold = Font(font_dir / "inter-latin-800.woff", font_dir / "inter-latin-ext-800.woff")
    semi = Font(font_dir / "inter-latin-600.woff", font_dir / "inter-latin-ext-600.woff")
    OUT.mkdir(parents=True, exist_ok=True)

    size, track = 38.0, -0.6           # wordmark: Inter ExtraBold, cap height ~ 27.6 (43% of the tile)
    w_aed = bold.width("AED ", size, track)
    w_all = bold.width("AED Radar", size, track)
    slogan = "Udostępnij dane. Daj komuś szansę."
    s_size = 13.0

    def wordmark(x: float, baseline: float, c1: str, c2: str) -> str:
        return (f'<path d="{bold.path("AED", x, baseline, size, track)}" fill="{c1}"/>'
                f'<path d="{bold.path("Radar", x + w_aed, baseline, size, track)}" fill="{c2}"/>')

    gap = 16.0
    base_h = 32 + 27.6 / 2            # cap height centred on the tile
    files = {}

    # Sygnet (app icon, favicon) and the sygnet without the tile.
    files["sygnet.svg"] = svg(64, 64, "AED Radar – sygnet", mark(GREEN, "#FFFFFF", RED))
    files["sygnet-bez-tla.svg"] = svg(64, 64, "AED Radar – sygnet bez tła", mark(None, GREEN, "#FFFFFF", 0.6).replace(
        f'<path d="{HEART}" fill="{GREEN}"/>', f'<path d="{HEART}" fill="{RED}"/>'))
    files["favicon.svg"] = svg(64, 64, "AED Radar", mark(GREEN, "#FFFFFF", RED, outer=False))

    # Horizontal: sygnet + wordmark.
    w = 64 + gap + w_all
    files["logo-poziome.svg"] = svg(w, 64, "AED Radar", mark(GREEN, "#FFFFFF", RED) + wordmark(64 + gap, base_h, NAVY, GREEN))

    # Horizontal with the slogan under the wordmark.
    s_w = semi.width(slogan, s_size)
    w2 = 64 + gap + max(w_all, s_w + 1) + 2  # slogan starts 1 unit in; 2 units so the last dot does not touch the edge
    files["logo-z-haslem.svg"] = svg(w2, 64, "AED Radar – Udostępnij dane. Daj komuś szansę.",
                                     mark(GREEN, "#FFFFFF", RED) + wordmark(64 + gap, 36, NAVY, GREEN)
                                     + f'<path d="{semi.path(slogan, 64 + gap + 1, 57, s_size)}" fill="{GREY}"/>')

    # Vertical: sygnet centred above the wordmark.
    vw = max(w_all, 96)
    files["logo-pionowe.svg"] = svg(vw, 64 + 14 + 28, "AED Radar",
                                    f'<g transform="translate({(vw - 64) / 2:.2f} 0)">{mark(GREEN, "#FFFFFF", RED)}</g>'
                                    + wordmark((vw - w_all) / 2, 64 + 14 + 27.6, NAVY, GREEN))

    # Single colour (black for print on light backgrounds, white on dark).
    for name, colour in (("logo-mono-czarne.svg", "#000000"), ("logo-mono-biale.svg", "#FFFFFF")):
        files[name] = svg(w, 64, "AED Radar", mark_mono(colour, "m") + wordmark(64 + gap, base_h, colour, colour))

    for name, content in files.items():
        (OUT / name).write_text(content, encoding="utf-8", newline="\n")
        print(OUT / name)


if __name__ == "__main__":
    main(Path(sys.argv[1]))
