"""Base class for all Playwright UI tests."""

import os
import time
from dataclasses import dataclass, field
from typing import Optional

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SCREENSHOT_DIR = os.path.join(SCRIPT_DIR, "..", "reports", "screenshots")


@dataclass
class UiTestCase:
    name: str
    passed: bool
    duration: float
    error: str = ""
    screenshot: str = ""  # path to failure screenshot


@dataclass
class UiTestSuiteResult:
    suite_name: str
    cases: list = field(default_factory=list)

    @property
    def passed(self) -> bool:
        return all(c.passed for c in self.cases)

    @property
    def total(self) -> int:
        return len(self.cases)

    @property
    def num_passed(self) -> int:
        return sum(1 for c in self.cases if c.passed)

    @property
    def num_failed(self) -> int:
        return self.total - self.num_passed

    @property
    def duration(self) -> float:
        return sum(c.duration for c in self.cases)


class UiTestBase:
    def __init__(self, frontend_url: str, headless: bool = False, slow_mo: int = 300):
        self.frontend_url = frontend_url.rstrip("/")
        self.headless = headless
        self.slow_mo = slow_mo if not headless else 0
        self._playwright = None
        self._browser = None
        self.page = None
        self._results = UiTestSuiteResult(suite_name=self.__class__.__name__)

    # ------------------------------------------------------------------
    # Lifecycle
    # ------------------------------------------------------------------

    def setup(self):
        from playwright.sync_api import sync_playwright
        os.makedirs(SCREENSHOT_DIR, exist_ok=True)
        self._playwright = sync_playwright().start()
        self._browser = self._playwright.chromium.launch(
            headless=self.headless,
            slow_mo=self.slow_mo,
            args=["--start-maximized"],
        )
        context = self._browser.new_context(
            viewport={"width": 1400, "height": 900},
            accept_downloads=True,
        )
        self.page = context.new_page()

    def teardown(self):
        try:
            if self._browser:
                self._browser.close()
        except Exception:
            pass
        try:
            if self._playwright:
                self._playwright.stop()
        except Exception:
            pass

    # ------------------------------------------------------------------
    # Navigation helpers
    # ------------------------------------------------------------------

    def navigate(self, path: str = "") -> bool:
        """Go to a URL and wait for Angular root to render."""
        url = f"{self.frontend_url}{path}"
        try:
            self.page.goto(url, wait_until="domcontentloaded", timeout=15000)
            self.page.wait_for_selector("app-root", state="attached", timeout=10000)
            # Small buffer for Angular change detection / HTTP calls
            self.page.wait_for_timeout(800)
            return True
        except Exception:
            return False

    def wait_for(self, selector: str, timeout: int = 6000) -> bool:
        """Wait for a selector to be visible. Returns False on timeout."""
        try:
            self.page.wait_for_selector(selector, state="visible", timeout=timeout)
            return True
        except Exception:
            return False

    # ------------------------------------------------------------------
    # Screenshot helper
    # ------------------------------------------------------------------

    def _screenshot(self, test_name: str) -> str:
        safe = "".join(c if c.isalnum() or c in "-_" else "_" for c in test_name)
        ts = int(time.time())
        path = os.path.join(SCREENSHOT_DIR, f"{safe}_{ts}.png")
        try:
            self.page.screenshot(path=path, full_page=True)
        except Exception:
            return ""
        return path

    # ------------------------------------------------------------------
    # Assert helpers
    # ------------------------------------------------------------------

    def _case(
        self,
        name: str,
        start: float,
        passed: bool,
        error: str = "",
        screenshot_on_fail: bool = True,
    ) -> UiTestCase:
        duration = time.time() - start
        shot = ""
        if not passed and screenshot_on_fail:
            shot = self._screenshot(name)
        tc = UiTestCase(name=name, passed=passed, duration=duration,
                        error=error, screenshot=shot)
        self._results.cases.append(tc)
        return tc

    def assert_visible(self, name: str, selector: str, timeout: int = 6000) -> UiTestCase:
        start = time.time()
        visible = self.wait_for(selector, timeout=timeout)
        return self._case(name, start, visible,
                          error="" if visible else f"Selector not visible: {selector}")

    def assert_url_contains(self, name: str, fragment: str) -> UiTestCase:
        start = time.time()
        try:
            self.page.wait_for_url(f"**{fragment}**", timeout=6000)
            passed = True
            error = ""
        except Exception:
            passed = False
            error = f"URL does not contain '{fragment}' — actual: {self.page.url}"
        return self._case(name, start, passed, error=error)

    def assert_text_visible(self, name: str, text: str, timeout: int = 6000) -> UiTestCase:
        start = time.time()
        try:
            self.page.wait_for_selector(f"text={text}", state="visible", timeout=timeout)
            passed = True
            error = ""
        except Exception:
            passed = False
            error = f"Text not found on page: '{text}'"
        return self._case(name, start, passed, error=error)

    def run(self) -> UiTestSuiteResult:
        raise NotImplementedError
