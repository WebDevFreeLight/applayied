#!/usr/bin/env python3
"""Bundle src/ into ONE self-contained index.html (CSS, JS and fonts inlined).

Why: reviewers often double-click index.html inside the ZIP. Windows then opens
only that file from a temp folder, so any relative css/js/font path breaks.
A single file works from anywhere, including straight out of the ZIP.

Usage:  python build.py      (edit files in src/, then rebuild)
"""
import base64, pathlib, re

SRC = pathlib.Path(__file__).parent / 'src'
html = (SRC / 'index.html').read_text(encoding='utf-8')

css = (SRC / 'css' / 'styles.css').read_text(encoding='utf-8')
def font(m):
    path = (SRC / 'css' / m.group(1)).resolve()
    data = base64.b64encode(path.read_bytes()).decode()
    return f"url('data:font/woff2;base64,{data}')"
css = re.sub(r"url\('(\.\./assets/fonts/[^']+\.woff2)'\)", font, css)
assert '../assets' not in css, 'unresolved font path'

html = html.replace('<link rel="stylesheet" href="css/styles.css">', '<style>\n' + css + '\n</style>')
def script(m):
    code = (SRC / m.group(1)).read_text(encoding='utf-8')
    assert '</script' not in code.lower()
    return f'<script>/* {m.group(1)} */\n{code}\n</script>'
html = re.sub(r'<script src="(js/[^"]+)"></script>', script, html)
assert 'src="js/' not in html and 'href="css/' not in html

out = pathlib.Path(__file__).parent / 'index.html'
out.write_text(html, encoding='utf-8')
print(f'wrote {out} ({len(html)//1024} KB, self-contained)')
