import os

from dotenv import load_dotenv

load_dotenv()

RECAM_API_BASE_URL = os.environ.get("RECAM_API_BASE_URL", "http://localhost:5262")
RECAM_EMAIL = os.environ.get("RECAM_EMAIL", "")
RECAM_PASSWORD = os.environ.get("RECAM_PASSWORD", "")
ANTHROPIC_API_KEY = os.environ.get("ANTHROPIC_API_KEY", "")
