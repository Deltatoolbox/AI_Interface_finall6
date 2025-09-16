from pathlib import Path
import os
import sys

def count_lines_in_file(file_path: Path) -> int:
    """Count lines in a file (binary-safe).

    Args:
        file_path (Path): Path to the file.

    Returns:
        int: Number of lines, including blank lines. Returns 0 on error.
    """
    if not file_path.suffix == ".cs" | file_path.suffix == ".json" | file_path.suffix == ".tsx" | file_path.suffix == ".ts" | file_path.suffix == ".jsx" | file_path.suffix == ".js" | file_path.suffix == ".md" | file_path.suffix == ".txt" | file_path.suffix == ".yaml" | file_path.suffix == ".yml" | file_path.suffix == ".html":
        return 0

    if file_path.is_symlink():
        return 0
    try:
        with file_path.open("rb") as f:
            return sum(1 for _ in f)
    except Exception:
        return 0

def count_total_lines(src_path: Path) -> int:
    """Count lines across all files under a directory recursively.

    Args:
        src_path (Path): Path to the root directory.

    Returns:
        int: Total number of lines across all files.
    """
    total = 0
    for dirpath, _, filenames in os.walk(src_path, followlinks=False):
        for name in filenames:
            total += count_lines_in_file(Path(dirpath) / name)
    return total

def main() -> None:
    """Locate 'src' next to this script and print the total line count as an integer."""
    base_dir = Path(__file__).resolve().parent
    src_dir = base_dir / "src"
    if not src_dir.is_dir():
        print("src directory not found", file=sys.stderr)
        sys.exit(1)
    print(count_total_lines(src_dir))

if __name__ == "__main__":
    main()
