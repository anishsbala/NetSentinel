from __future__ import annotations

import sys
from pathlib import Path

PYTHON_ROOT = Path(__file__).parents[1]
sys.path.insert(0, str(PYTHON_ROOT / "scanner"))
sys.path.insert(0, str(PYTHON_ROOT / "agent"))
