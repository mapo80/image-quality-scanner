import sys
import cv2
from brisque import BRISQUE

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("nan")
        raise SystemExit(1)
    img = cv2.imread(sys.argv[1])
    if img is None:
        print("nan")
    else:
        rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        score = BRISQUE().score(rgb)
        print(float(score))
