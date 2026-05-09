"""Generate Morandi-style ICO for TokenUsageMonitor."""
import struct
import io
from PIL import Image, ImageDraw

# Morandi palette (muted, low-saturation)
MORANDI_BG = (140, 155, 168)       # Dusty blue-grey background
MORANDI_RING_BG = (180, 190, 198)  # Lighter ring track
MORANDI_RING_FG = (245, 245, 247)  # Off-white active segment
MORANDI_CENTER = (120, 135, 148)   # Slightly darker center dot


def draw_donut_chart(size, fill_pct=0.65):
    """Draw a donut chart icon at given size."""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    padding = max(size // 8, 2)
    bg_rect = [padding, padding, size - padding, size - padding]
    radius = (size - 2 * padding) // 2

    # Background rounded rect
    draw.rounded_rectangle(bg_rect, radius=radius, fill=MORANDI_BG)

    # Donut ring parameters
    cx = cy = size // 2
    outer_r = int(size * 0.30)
    inner_r = int(size * 0.18)
    stroke = outer_r - inner_r

    # Background ring (full circle)
    draw.arc(
        [cx - outer_r, cy - outer_r, cx + outer_r, cy + outer_r],
        start=0, end=360,
        fill=MORANDI_RING_BG, width=stroke
    )

    # Foreground arc (fill percentage) - clockwise from top
    end_angle = int(360 * fill_pct)
    draw.arc(
        [cx - outer_r, cy - outer_r, cx + outer_r, cy + outer_r],
        start=90, end=90 - end_angle,
        fill=MORANDI_RING_FG, width=stroke
    )

    # Center dot
    dot_r = max(size // 32, 2)
    draw.ellipse(
        [cx - dot_r, cy - dot_r, cx + dot_r, cy + dot_r],
        fill=MORANDI_CENTER
    )

    return img


def save_multi_ico(images, output_path):
    """Manually build a multi-resolution ICO file from PIL Images."""
    # ICO header: Reserved(2) + Type(2) + Count(2)
    ico_header = struct.pack("<HHH", 0, 1, len(images))

    # Each image needs an ICONDIRENTRY (16 bytes)
    # Width(1) + Height(1) + Colors(1) + Reserved(1) + Planes(2) + BitDepth(2) + Size(4) + Offset(4)
    entries = b""
    data_blocks = b""
    offset = 6 + 16 * len(images)  # Header size + all entry sizes

    for img in images:
        w, h = img.size
        # Save as PNG inside ICO for best quality (Windows Vista+ supports this)
        buf = io.BytesIO()
        img.save(buf, format="PNG")
        png_data = buf.getvalue()

        # ICONDIRENTRY
        # Width/Height: 0 means 256
        entry_w = 0 if w >= 256 else w
        entry_h = 0 if h >= 256 else h
        entries += struct.pack(
            "<BBBBHHII",
            entry_w, entry_h,   # Width, Height
            0,                  # Color count (0 = >256)
            0,                  # Reserved
            1,                  # Color planes
            32,                 # Bits per pixel
            len(png_data),      # Data size
            offset              # Data offset
        )
        data_blocks += png_data
        offset += len(png_data)

    with open(output_path, "wb") as f:
        f.write(ico_header)
        f.write(entries)
        f.write(data_blocks)


def main():
    sizes = [16, 32, 48, 64, 128, 256]
    images = []
    for sz in sizes:
        img = draw_donut_chart(sz, fill_pct=0.65)
        images.append(img)
        print(f"Generated {sz}x{sz}")

    output_path = r"N:\Data\Project\FULL-prooject\TokenUsageMonitor\src\TokenUsageMonitor\Assets\app.ico"
    save_multi_ico(images, output_path)

    import os
    file_size = os.path.getsize(output_path)
    print(f"Saved ICO to {output_path} ({file_size} bytes)")

    # Verify
    ico = Image.open(output_path)
    idx = 0
    while True:
        print(f"  Frame {idx}: {ico.size}")
        idx += 1
        try:
            ico.seek(idx)
        except EOFError:
            break


if __name__ == "__main__":
    main()
