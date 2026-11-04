#!/usr/bin/env python3
"""Generate the checked-in PixelMate application icon assets."""

from pathlib import Path
from struct import pack
from io import BytesIO

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parent.parent
ASSETS = ROOT / "src" / "PixelMate" / "Assets"
CANVAS_SIZE = 1024


def rounded_rectangle(draw, box, radius, fill=None, outline=None, width=1):
    draw.rounded_rectangle(box, radius=radius, fill=fill, outline=outline, width=width)


def make_icon() -> Image.Image:
    image = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    rounded_rectangle(
        draw,
        (52, 52, 972, 972),
        radius=220,
        fill="#11141A",
        outline="#455063",
        width=18,
    )
    rounded_rectangle(
        draw,
        (116, 116, 908, 908),
        radius=164,
        fill="#1B222D",
    )

    pixel_size = 83
    pixel_gap = 16
    start_x = 208
    start_y = 214
    colors = [
        ["#2C3A4D", "#3F5B7C", "#2D4868"],
        ["#3A526B", "#4B8FD2", "#3E70A2"],
        ["#31465D", "#417AAA", "#344E6B"],
    ]
    for row, row_colors in enumerate(colors):
        for column, color in enumerate(row_colors):
            left = start_x + column * (pixel_size + pixel_gap)
            top = start_y + row * (pixel_size + pixel_gap)
            rounded_rectangle(
                draw,
                (left, top, left + pixel_size, top + pixel_size),
                radius=15,
                fill=color,
            )

    line_start = (274, 764)
    line_end = (760, 296)
    draw.line((line_start, line_end), fill="#5A2B08", width=56)
    draw.line((line_start, line_end), fill="#FFB454", width=28)

    for center in (line_start, line_end):
        x, y = center
        draw.ellipse((x - 54, y - 54, x + 54, y + 54), fill="#161B24")
        draw.ellipse((x - 39, y - 39, x + 39, y + 39), fill="#FFBE68")
        draw.ellipse((x - 18, y - 18, x + 18, y + 18), fill="#FFF1D9")

    return image


def main() -> None:
    ASSETS.mkdir(parents=True, exist_ok=True)
    icon = make_icon()
    icon.resize((256, 256), Image.Resampling.LANCZOS).save(
        ASSETS / "PixelMate.png",
        optimize=True,
    )

    ico_sizes = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    icon.save(
        ASSETS / "PixelMate.ico",
        format="ICO",
        sizes=ico_sizes,
        bitmap_format="png",
    )

    icns_chunks = []
    for chunk_type, size in (
        (b"icp4", 16),
        (b"icp5", 32),
        (b"icp6", 64),
        (b"ic07", 128),
        (b"ic08", 256),
        (b"ic09", 512),
        (b"ic10", 1024),
    ):
        buffer = BytesIO()
        icon.resize((size, size), Image.Resampling.LANCZOS).save(
            buffer,
            format="PNG",
            optimize=True,
        )
        payload = buffer.getvalue()
        icns_chunks.append(chunk_type + pack(">I", len(payload) + 8) + payload)

    icns_body = b"".join(icns_chunks)
    (ASSETS / "PixelMate.icns").write_bytes(
        b"icns" + pack(">I", len(icns_body) + 8) + icns_body
    )


if __name__ == "__main__":
    main()
