"""
MerxoSell Automation Test Suite
=================================
Usage:
  python run_all.py                    # run everything
  python run_all.py --api-only         # only API tests
  python run_all.py --unit-only        # only dotnet + ng unit tests
  python run_all.py --skip-build       # skip dotnet build step
  python run_all.py --env production   # use production config block
"""

import argparse
import datetime
import json
import os
import sys
import time

import yaml
from colorama import Fore, Style, init as colorama_init

colorama_init(autoreset=True)

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))


# ---------------------------------------------------------------------------
# Config loader
# ---------------------------------------------------------------------------

def load_config(env: str) -> dict:
    config_path = os.path.join(SCRIPT_DIR, "config.yaml")
    with open(config_path) as f:
        raw = yaml.safe_load(f)
    active = env or raw.get("environment", {}).get("active", "development")
    cfg = raw.get(active, raw.get("development", {}))
    cfg["credentials"] = raw.get("test_credentials", {})
    cfg["timeouts"] = raw.get("timeouts", {})
    cfg["reports"] = raw.get("reports", {"output_dir": "./reports", "keep_last_n": 10})
    cfg["active_env"] = active
    return cfg


# ---------------------------------------------------------------------------
# Console helpers
# ---------------------------------------------------------------------------

def _print_header(text: str):
    bar = "=" * 70
    print(f"\n{Fore.CYAN}{bar}")
    print(f"  {text}")
    print(f"{bar}{Style.RESET_ALL}")


def _print_suite_header(name: str):
    print(f"\n{Fore.YELLOW}>> {name}{Style.RESET_ALL}")
    print(f"{Fore.YELLOW}{'-' * 60}{Style.RESET_ALL}")


def _print_case(name: str, passed: bool, duration: float, error: str = ""):
    icon = f"{Fore.GREEN}[+]" if passed else f"{Fore.RED}[x]"
    status = f"{Fore.GREEN}PASS" if passed else f"{Fore.RED}FAIL"
    print(f"  {icon}  {status}  {Style.RESET_ALL}{name}  {Fore.WHITE}({duration:.2f}s){Style.RESET_ALL}")
    if error:
        print(f"       {Fore.RED}{error}{Style.RESET_ALL}")


def _print_runner_result(result, label: str):
    _print_suite_header(label)
    icon = f"{Fore.GREEN}[+]" if result.passed else f"{Fore.RED}[x]"
    status = "PASS" if result.passed else "FAIL"
    color = Fore.GREEN if result.passed else Fore.RED
    print(f"  {icon}  {color}{status}{Style.RESET_ALL}  ({result.duration:.1f}s)")
    if result.tests_total:
        print(f"       Tests: {result.tests_total} total / "
              f"{Fore.GREEN}{result.tests_passed} passed{Style.RESET_ALL} / "
              f"{Fore.RED}{result.tests_failed} failed{Style.RESET_ALL}")
    if not result.passed and result.error:
        for line in result.error.splitlines()[-20:]:
            print(f"       {Fore.RED}{line}{Style.RESET_ALL}")


# ---------------------------------------------------------------------------
# HTML report generator
# ---------------------------------------------------------------------------

HTML_TEMPLATE = """<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>MerxoSell Automation Report — {run_date}</title>
<style>
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background:#f5f6fa; margin:0; padding:20px; }}
  h1 {{ color:#2c3e50; }}
  .summary {{ display:flex; gap:20px; margin:20px 0; flex-wrap:wrap; }}
  .card {{ background:#fff; border-radius:8px; padding:20px 28px; box-shadow:0 2px 6px rgba(0,0,0,.1); min-width:140px; text-align:center; }}
  .card .num {{ font-size:2.2em; font-weight:700; }}
  .card .lbl {{ font-size:.85em; color:#666; }}
  .pass {{ color:#27ae60; }} .fail {{ color:#e74c3c; }} .neutral {{ color:#2980b9; }}
  .suite {{ background:#fff; border-radius:8px; padding:16px 20px; margin:16px 0; box-shadow:0 1px 4px rgba(0,0,0,.08); }}
  .suite h2 {{ margin:0 0 12px; font-size:1.05em; color:#34495e; }}
  table {{ width:100%; border-collapse:collapse; }}
  th {{ text-align:left; font-size:.8em; color:#888; border-bottom:1px solid #eee; padding:4px 8px; }}
  td {{ padding:6px 8px; font-size:.9em; border-bottom:1px solid #f0f0f0; }}
  .badge {{ display:inline-block; padding:2px 10px; border-radius:12px; font-size:.8em; font-weight:600; }}
  .badge-pass {{ background:#d5f5e3; color:#27ae60; }}
  .badge-fail {{ background:#fadbd8; color:#e74c3c; }}
  .err {{ color:#c0392b; font-size:.82em; }}
  .meta {{ font-size:.82em; color:#999; margin-top:24px; }}
</style>
</head>
<body>
<h1>MerxoSell Automation Report</h1>
<p>Environment: <strong>{env}</strong> &nbsp;|&nbsp; Run: <strong>{run_date}</strong> &nbsp;|&nbsp; Duration: <strong>{total_duration:.1f}s</strong></p>

<div class="summary">
  <div class="card"><div class="num neutral">{total_tests}</div><div class="lbl">Total Tests</div></div>
  <div class="card"><div class="num pass">{total_passed}</div><div class="lbl">Passed</div></div>
  <div class="card"><div class="num fail">{total_failed}</div><div class="lbl">Failed</div></div>
  <div class="card"><div class="num {overall_color}">{overall_label}</div><div class="lbl">Overall</div></div>
</div>

{suites_html}

<p class="meta">Generated by MerxoSell Python Automation Suite</p>
</body>
</html>"""

SUITE_TEMPLATE = """<div class="suite">
<h2>{suite_name} &nbsp; <span class="badge badge-{badge}">{badge_label}</span>
  &nbsp; <span style="font-weight:normal;font-size:.9em;color:#888">{passed}/{total} passed &nbsp; {duration:.1f}s</span>
</h2>
<table>
<tr><th>Test</th><th>Result</th><th>Duration</th><th>Detail</th></tr>
{rows}
</table>
</div>"""

ROW_TEMPLATE = """<tr>
  <td>{name}</td>
  <td><span class="badge badge-{badge}">{label}</span></td>
  <td>{duration:.2f}s</td>
  <td class="err">{error}</td>
</tr>"""


def _build_html(all_results: list, env: str, total_duration: float) -> str:
    run_date = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    total = passed = failed = 0

    suites_html_parts = []
    for suite in all_results:
        s_total = getattr(suite, "total", 0)
        s_passed = getattr(suite, "num_passed", 0)
        s_failed = getattr(suite, "num_failed", 0)
        s_duration = getattr(suite, "duration", 0.0)
        suite_name = getattr(suite, "suite_name", str(suite))
        cases = getattr(suite, "cases", [])
        total += s_total
        passed += s_passed
        failed += s_failed

        rows = ""
        for c in cases:
            shot = getattr(c, "screenshot", "")
            shot_link = (f' <a href="{shot}" target="_blank">screenshot</a>'
                         if shot else "")
            rows += ROW_TEMPLATE.format(
                name=c.name,
                badge="pass" if c.passed else "fail",
                label="PASS" if c.passed else "FAIL",
                duration=c.duration,
                error=(c.error[:120] if c.error else "") + shot_link,
            )
        badge = "pass" if s_failed == 0 else "fail"
        suites_html_parts.append(SUITE_TEMPLATE.format(
            suite_name=suite_name,
            badge=badge,
            badge_label="PASS" if badge == "pass" else "FAIL",
            passed=s_passed,
            total=s_total,
            duration=s_duration,
            rows=rows,
        ))

    overall_ok = (failed == 0)
    return HTML_TEMPLATE.format(
        run_date=run_date,
        env=env,
        total_duration=total_duration,
        total_tests=total,
        total_passed=passed,
        total_failed=failed,
        overall_color="pass" if overall_ok else "fail",
        overall_label="PASS" if overall_ok else "FAIL",
        suites_html="".join(suites_html_parts),
    )


def _save_report(html: str, reports_cfg: dict) -> str:
    output_dir = os.path.join(SCRIPT_DIR, reports_cfg.get("output_dir", "./reports"))
    os.makedirs(output_dir, exist_ok=True)
    ts = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    path = os.path.join(output_dir, f"report_{ts}.html")
    with open(path, "w", encoding="utf-8") as f:
        f.write(html)

    # Rotate old reports
    keep = reports_cfg.get("keep_last_n", 10)
    reports = sorted(
        [os.path.join(output_dir, x) for x in os.listdir(output_dir) if x.endswith(".html")]
    )
    for old in reports[:-keep]:
        try:
            os.remove(old)
        except OSError:
            pass
    return path


# ---------------------------------------------------------------------------
# Main orchestrator
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="MerxoSell full-project automation runner")
    parser.add_argument("--env", default=None, help="Config environment block (default: active in config.yaml)")
    parser.add_argument("--api-only", action="store_true", help="Run only API tests")
    parser.add_argument("--unit-only", action="store_true", help="Run only dotnet/ng unit tests")
    parser.add_argument("--ui-only", action="store_true", help="Run only browser UI tests")
    parser.add_argument("--skip-build", action="store_true", help="Skip dotnet build step")
    parser.add_argument("--skip-frontend", action="store_true", help="Skip ng build/test steps")
    parser.add_argument("--skip-ui", action="store_true", help="Skip browser UI tests")
    parser.add_argument("--headless", action="store_true", help="Run browser tests headless (no visible window)")
    parser.add_argument("--no-report", action="store_true", help="Skip HTML report generation")
    args = parser.parse_args()

    cfg = load_config(args.env)
    base_url = cfg["api_base_url"]
    creds = cfg["credentials"]
    timeouts = cfg["timeouts"]

    _print_header(f"MerxoSell Automation Suite  |  env={cfg['active_env']}  |  {datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")

    global_start = time.time()
    all_suite_results = []
    runner_results = []  # for non-TestSuiteResult items (build/unit runners)

    # -----------------------------------------------------------------------
    # 1. Backend build
    # -----------------------------------------------------------------------
    if not args.api_only and not args.skip_build:
        from runners.backend_runner import run_backend_build
        _print_suite_header("Backend Build (dotnet build)")
        print(f"  {Fore.WHITE}Project: {cfg['backend_project']}{Style.RESET_ALL}")
        result = run_backend_build(
            os.path.join(SCRIPT_DIR, cfg["backend_project"]),
            timeout=timeouts.get("dotnet_build", 120),
        )
        _print_runner_result(result, result.name)
        runner_results.append(result)
        if not result.passed:
            print(f"\n{Fore.RED}Build failed - aborting test run.{Style.RESET_ALL}")
            sys.exit(1)

    # -----------------------------------------------------------------------
    # 2. Backend unit + integration tests
    # -----------------------------------------------------------------------
    if not args.api_only:
        from runners.backend_runner import run_backend_tests
        result = run_backend_tests(
            os.path.join(SCRIPT_DIR, cfg["backend_tests_project"]),
            timeout=timeouts.get("dotnet_test", 300),
        )
        _print_runner_result(result, result.name)
        runner_results.append(result)

    # -----------------------------------------------------------------------
    # 3. Frontend build + tests
    # -----------------------------------------------------------------------
    if not args.api_only and not args.skip_frontend:
        from runners.frontend_runner import run_frontend_build, run_frontend_tests
        fe_path = os.path.join(SCRIPT_DIR, cfg["frontend_project"])

        build_result = run_frontend_build(fe_path, timeout=timeouts.get("ng_build", 180))
        _print_runner_result(build_result, build_result.name)
        runner_results.append(build_result)

        test_result = run_frontend_tests(fe_path, timeout=timeouts.get("ng_test", 120))
        _print_runner_result(test_result, test_result.name)
        runner_results.append(test_result)

    # -----------------------------------------------------------------------
    # 4. API tests (only when NOT unit-only / ui-only)
    # -----------------------------------------------------------------------
    if not args.unit_only and not args.ui_only:
        from api_tests.test_health import HealthTests
        from api_tests.test_auth import AuthTests
        from api_tests.test_products import ProductTests
        from api_tests.test_cart import CartTests
        from api_tests.test_orders import OrderTests
        from api_tests.test_admin import AdminTests

        suites = [
            HealthTests(base_url, timeout=timeouts.get("api_request", 30)),
            AuthTests(base_url, creds, timeout=timeouts.get("api_request", 30)),
            ProductTests(base_url, creds, timeout=timeouts.get("api_request", 30)),
            CartTests(base_url, creds, timeout=timeouts.get("api_request", 30)),
            OrderTests(base_url, creds, timeout=timeouts.get("api_request", 30)),
            AdminTests(base_url, creds, timeout=timeouts.get("api_request", 30)),
        ]

        for suite in suites:
            _print_suite_header(suite.__class__.__name__)
            suite_result = suite.run()
            all_suite_results.append(suite_result)
            for case in suite_result.cases:
                _print_case(case.name, case.passed, case.duration, case.error)
            color = Fore.GREEN if suite_result.passed else Fore.RED
            print(f"\n  {color}Suite total: {suite_result.num_passed}/{suite_result.total} passed{Style.RESET_ALL}")

    # -----------------------------------------------------------------------
    # 5. Browser UI tests
    # -----------------------------------------------------------------------
    if not args.unit_only and not args.api_only and not args.skip_ui:
        frontend_url = cfg.get("frontend_url", "http://localhost:4200")
        headless = args.headless
        slow_mo = 0 if headless else 800

        try:
            from ui_tests.test_home import HomeUiTests
            from ui_tests.test_products import ProductsUiTests
            from ui_tests.test_auth import AuthUiTests
            from ui_tests.test_cart import CartUiTests
            from ui_tests.test_seller import SellerUiTests
            from ui_tests.test_admin import AdminUiTests

            ui_suites = [
                HomeUiTests(frontend_url, headless=headless, slow_mo=slow_mo),
                ProductsUiTests(frontend_url, headless=headless, slow_mo=slow_mo),
                AuthUiTests(frontend_url, creds, headless=headless, slow_mo=slow_mo),
                CartUiTests(frontend_url, creds, headless=headless, slow_mo=slow_mo),
                SellerUiTests(frontend_url, creds, headless=headless, slow_mo=slow_mo),
                AdminUiTests(frontend_url, creds, headless=headless, slow_mo=slow_mo),
            ]

            mode_label = "headless" if headless else "headed browser"
            _print_header(f"Browser UI Tests  ({mode_label}  |  {frontend_url})")

            for suite in ui_suites:
                _print_suite_header(suite.__class__.__name__)
                try:
                    suite.setup()
                    suite_result = suite.run()
                finally:
                    suite.teardown()

                all_suite_results.append(suite_result)
                for case in suite_result.cases:
                    shot_hint = f"  [screenshot: {case.screenshot}]" if case.screenshot else ""
                    _print_case(case.name, case.passed, case.duration,
                                case.error + shot_hint)
                color = Fore.GREEN if suite_result.passed else Fore.RED
                print(f"\n  {color}Suite total: {suite_result.num_passed}/{suite_result.total} passed{Style.RESET_ALL}")

        except ImportError as e:
            print(f"\n{Fore.YELLOW}  [!]  Playwright not available — skipping UI tests: {e}{Style.RESET_ALL}")
            print(f"       Run: pip install playwright && python -m playwright install chromium")

    # -----------------------------------------------------------------------
    # 6. Final summary
    # -----------------------------------------------------------------------
    total_duration = time.time() - global_start
    _print_header("FINAL SUMMARY")

    all_passed = all(r.passed for r in runner_results)
    suite_passed = all(s.passed for s in all_suite_results)
    overall = all_passed and suite_passed

    for r in runner_results:
        color = Fore.GREEN if r.passed else Fore.RED
        mark = "[+]" if r.passed else "[x]"
        print(f"  {color}{mark}  {r.name}{Style.RESET_ALL}")

    for s in all_suite_results:
        color = Fore.GREEN if s.passed else Fore.RED
        mark = "[+]" if s.passed else "[x]"
        print(f"  {color}{mark}  {s.suite_name}  ({s.num_passed}/{s.total}){Style.RESET_ALL}")

    total_tests = sum(s.total for s in all_suite_results)
    total_passed = sum(s.num_passed for s in all_suite_results)
    total_failed = sum(s.num_failed for s in all_suite_results)

    print(f"\n  Tests     : {total_passed}/{total_tests} passed, {total_failed} failed")
    print(f"  Duration  : {total_duration:.1f}s")

    if overall:
        print(f"\n{Fore.GREEN}  [+]  ALL CHECKS PASSED{Style.RESET_ALL}")
    else:
        print(f"\n{Fore.RED}  [x]  SOME CHECKS FAILED -- see details above{Style.RESET_ALL}")

    # -----------------------------------------------------------------------
    # 6. HTML report
    # -----------------------------------------------------------------------
    if not args.no_report and all_suite_results:
        html = _build_html(all_suite_results, cfg["active_env"], total_duration)
        report_path = _save_report(html, cfg["reports"])
        print(f"\n  Report saved -> {Fore.CYAN}{report_path}{Style.RESET_ALL}")

    sys.exit(0 if overall else 1)


if __name__ == "__main__":
    main()
