# Roadmap técnico

**Estado atual — 28/10/2026:** v0.4.0 Alpha, Fase 2 iniciada a pedido do usuário.
Motor genérico de conjuração, observador de acertos e técnicas básicas em
código; lista de técnicas integrada ao status da classe. Formação natural
das três tradições e demonstração visual inicial da Fase 3 adicionadas.
Compilação e 99 cenários de regras aprovados. Pactos/linhagens ampliados na
Fase 4; persistência básica da Fase 5 testada após salvar, encerrar e reabrir
o WorldBox: 28 Actors restaurados sem diferenças nos campos comparados.
Aceites de combate da Fase 1 e testes das quatro armas
continuam abertos. Ver `PACTOS_LINHAGENS_E_PERSISTENCIA.md` e `FASE_2.md`.
Conferidos no WorldBox: lista Corte preciso I junto dos status após treino
manual e uma Centelha Rubra com HP 73→66, mana -3 e recarga registrada de 2 s.
No teste de 28/10, um personagem aprendeu Centelha Rubra por estudo próprio,
e as abas foram abertas/alternadas após a correção do organizador nativo.
Na expansão posterior, magos, bruxos e feiticeiro aprenderam e conjuraram
automaticamente contra ursos, com dano confirmado. Isso ainda não valida
todos os repertórios, treino armado ou carga. Ver `FORMACAO_NATURAL_E_FX.md`.
Ver `VALIDACAO_APRENDIZADO.md`, `ATRIBUTOS_E_FORMACAO.md` e o roadmap completo.

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
- [x] Implementar persistência básica de mana, XP, círculo e spells por ID imutável do Actor.
  Código versionado, 25 cenários novos e salvar/reabrir no jogo aprovados para
  os 28 Actors do cenário documentado. Aceite nativo de recargas, buffs e valor
  imediato de aura permanece pendente; herança e instituições também.
- [ ] IA para decidir quando e qual magia usar e limitar efeitos em massa.
- [ ] Preparar perfis de aptidão natural por criatura: escolas, círculo,
  repertório inato e afinidades marciais. Exemplos solicitados: dragões com
  círculos elevados e demônios com magia e espada. Ainda não implementado;
  detalhes e aceites na seção 6.3 do roadmap completo.

## Etapa D — Amplitude
- [ ] Círculos 2–9 com mecânicas específicas.
- [ ] Especialização de classes: Mago, Bruxo, Feiticeiro.
- [ ] Combos e animações de espada/aura com níveis de qualidade de FX configuráveis.
- [ ] Balanceamento, tradução, documentação e release.

**Definição de pronto:** cada funcionalidade só é declarada implementada quando compilou,
foi testada na versão alvo e passou pelo cenário correspondente em `TESTES.md`.
