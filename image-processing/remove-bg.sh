#!/usr/bin/env bash
# Remove backgrounds from every image in ./input/ and save PNGs to ./output/
# Usage:   ./remove-bg.sh              (uses birefnet-general, best quality)
#          ./remove-bg.sh isnet-general-use   (alternative model, faster)

set -e
cd "$(dirname "$0")"

MODEL="${1:-birefnet-general}"
INPUT_DIR="./input"
OUTPUT_DIR="./output"

if ! python -c "import rembg" &>/dev/null; then
  echo "ERROR: rembg not installed in Python environment."
  echo "Install with:  pip install \"rembg[cpu]\""
  exit 1
fi

shopt -s nullglob nocaseglob
files=("$INPUT_DIR"/*.{jpg,jpeg,png,webp})
shopt -u nocaseglob

if [ ${#files[@]} -eq 0 ]; then
  echo "No images found in $INPUT_DIR. Drop files there and run again."
  exit 0
fi

echo "Processing ${#files[@]} image(s) with model '$MODEL'..."
for f in "${files[@]}"; do
  base="$(basename "${f%.*}")"
  out="$OUTPUT_DIR/${base}.png"
  echo "  -> $base.png"
  python -c "
import sys
from rembg import remove, new_session
session = new_session('$MODEL')
with open(sys.argv[1], 'rb') as i, open(sys.argv[2], 'wb') as o:
    o.write(remove(i.read(), session=session))
" "$f" "$out"
done

echo "Done. Clean PNGs are in $OUTPUT_DIR/"
