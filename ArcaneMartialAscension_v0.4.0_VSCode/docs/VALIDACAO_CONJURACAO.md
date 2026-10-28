# Validação da correção de conjuração — 27/10/2026

## Problema e correção

O usuário observou ausência de magia mesmo após preparar feitiços. O log antigo
registrava `OK: Lança Elétrica | mana -10`, mas essa indicação só refletia o
retorno do adaptador experimental: não havia projétil, e o adaptador procurava
um `getHit` com um parâmetro que não corresponde à API instalada.

- `CombatBridge` usa `Actor.getHit(float, bool, AttackType, BaseSimObject,
  bool, bool, bool)`, com atacante, redução de dano nativa e `pSkipIfShake=false`.
- O efeito só é considerado aplicado se `getHealth()` diminuir. O fallback
  que escrevia diretamente em HP foi removido.
- Magias ofensivas válidas criam um projétil; o impacto chama a API nativa.
  Um conjurador mantém no máximo uma magia em voo. Mana e recarga só são
  cobradas após o impacto confirmado. Alvo morto/interrompido ou protegido
  preserva os recursos quando nenhum dano foi aplicado.
- Posição vem de `BaseSimObject.cur_transform_position`, usada pelo renderer
  do Actor; seleção F8 e projeção de efeitos usam `World.world.camera`.
- O combate automático acompanha `Actor.attack_target` a cada 0,25 s,
  processando até 32 personagens por lote, sem esperar um ataque comum.
  Respeita relações de combate, alcance, conhecimentos, círculo, mana e recarga.
- A magia preparada no Grimório tem preferência; outras ofensivas conhecidas
  podem ser usadas quando a preferida não estiver disponível.
- O resultado manual aparece em tela mesmo com o painel F6 oculto. O Grimório
  mostra a situação da conjuração. Jogo pausado recusa novas conjurações e
  congela projéteis já em voo.
- Sprites e trilhas são visuais; falhas neles não invalidam dano já aplicado.

## Ambiente e backups

- WorldBox 0.51.2, NeoModLoader 1.2.1.1; DLLs reais da instalação local.
- Roslyn do NML 4.9.0.0: 13 fontes C#, zero erros e avisos.
- Backup atualizado: `C:\Users\rn558\Documents\AMA_TestBackups\SpellFix_20261027_220734`.
- Mod anterior: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261027_220916`.

## Cenários

- [x] Compilação contra as DLLs locais e `git diff --check`.
- [ ] Carregamento da revisão no NML sem erros do AMA.
- [ ] Conjuração manual em mundo descartável: trajetória visível, HP menor,
  mana descontada uma vez, recarga registrada após o impacto.
- [ ] Ausência de alvo: mensagem clara, sem cobrança.
- [ ] Jogo pausado: mensagem clara, sem cobrança.
- [ ] Conjuração automática contra inimigo: lançamento sem pressionar F8.
- [ ] Magia não implementada: sem cobrança.

## Limites

A trajetória e o dano são uma base comum para as magias ofensivas iniciais.
Isso não completa mecânicas exclusivas como lentidão, eletricidade, IA de
aprendizado, invocações ou os círculos avançados. Progressão segue em memória.
Magias de suporte permanecem protótipos da revisão anterior. Grandes batalhas
e persistência não fazem parte desta validação.
