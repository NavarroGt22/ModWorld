"""Verifica os dados do grimorio sem dependencia de DLLs do WorldBox.
Execucao opcional: python scripts/quickcheck.py
"""
import json
import pathlib
import re
import collections

root = pathlib.Path(__file__).resolve().parents[1] / 'ArcaneMartialAscension'
catalog = json.loads((root / 'Config/catalogo_magias_160.json').read_text(encoding='utf-8'))
manifest = json.loads((root / 'mod.json').read_text(encoding='utf-8'))
spells = catalog['magias']
assert manifest['version'] == '0.4.0'
assert len(spells) == 160
ids = [x['id'] for x in spells]
assert len(set(ids)) == 160
counts = collections.Counter((x['escola_id'], int(x['circulo'])) for x in spells)
assert len(counts) == 80 and all(n == 2 for n in counts.values())
cs = (root / 'Code/Spells/SpellCatalog.cs').read_text(encoding='utf-8')
csids = re.findall(r'Add\(new SpellEntry\("([^"]+)"', cs)
assert len(csids) == 160 and set(csids) == set(ids)
for school in ('abjuration','conjuration','divination','enchantment',
               'evocation','illusion','necromancy','transmutation'):
    assert (root / f'GameResources/ui/icons/ama/{school}.png').is_file()
print('OK: v0.4.0; 160 IDs; 8 escolas x 10 circulos x 2 magias; C# sincronizado; iconografia presente.')
print('Nota: nao testa compilacao C#, Unity nem execucao no WorldBox.')
