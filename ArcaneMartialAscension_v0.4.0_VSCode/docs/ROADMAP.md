# Roadmap técnico

**Estado atual — 27/10/2026:** v0.4.0 Alpha, Fase 1 em validação no roadmap
completo. Compilação, carga inicial e F5 confirmados; última correção de magia
ainda sem aceite de combate. Maestria e aprendizado autônomo têm protótipos
em código, com 24 cenários de regras aprovados. Persistência pendente.
Ver `VALIDACAO_APRENDIZADO.md` e a seção de estado do roadmap completo.

## Etapa A — Compatibilidade 0.51.2 (bloqueador)
- [x] Compilar sem warnings críticos/erros com NeoModLoader 1.2.1.1 e `Assembly-CSharp` correto. Evidência: `VALIDACAO_FASE_0.md`.
- [x] Confirmar `OnModLoad` e registro inicial de escolas/traços (log e painel na Fase 0).
- [ ] Validar callbacks de ataque e suas consequências em combate real.
- [ ] Resolver posição real de Actor e seleção visual de unidade.

## Etapa B — Funcionalidade demonstrável
- [ ] Fazer F5 e círculos na posição correta do mundo aparecerem consistentemente.
- [ ] Conjurar 1 magia com efeito real, dano correto, cooldown, mana e alvo.
- [ ] Maestria de espada com XP real em combate; repetir para lança/arco/machado.
- [ ] Testar dano/defesa em simulação com 20, 200 e 1000 unidades.

## Etapa C — Base de jogo
- [ ] Extrair executor genérico de magias (dano, cura, buff, debuff, área, projétil).
- [ ] Implementar todas as magias dos círculos 0 e 1 com regras específicas.
- [ ] Implementar persistência de mana, XP, círculo e spells por ID imutável do Actor.
- [ ] IA para decidir quando e qual magia usar e limitar efeitos em massa.

## Etapa D — Amplitude
- [ ] Círculos 2–9 com mecânicas específicas.
- [ ] Especialização de classes: Mago, Bruxo, Feiticeiro.
- [ ] Combos e animações de espada/aura com níveis de qualidade de FX configuráveis.
- [ ] Balanceamento, tradução, documentação e release.

**Definição de pronto:** cada funcionalidade só é declarada implementada quando compilou,
foi testada na versão alvo e passou pelo cenário correspondente em `TESTES.md`.
