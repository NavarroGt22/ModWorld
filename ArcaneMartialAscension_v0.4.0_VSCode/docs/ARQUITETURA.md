# Arquitetura v0.4.0 Alpha

**Entrada**: `Code/Main.cs`, classe `Main : BasicMod<Main>`.
O NML lê `mod.json`, compila a pasta `Code` e cria a instância no Unity.

- **Core/** — `ArcaneProgression`, `CharacterProgression`: mana/aura, círculos, XP de armas.
- **Core/TraitAttributes.cs** — perfis distintos de atributos por tradição,
  escola, arma e nível 1–7. O registro nativo e a ficha consultam a mesma fonte.
- **Core/ArcaneOverview.cs** — projeção de leitura por escola: magias conhecidas
  e maior círculo aprendido, separado da capacidade global do conjurador.
- **Spells/SpellCatalog.cs** — 160 feitiços embutidos em C#, com IDs estáveis. O JSON é documentação,
  não participa do carregamento. Para editar spells, sincronize os dois por ID e rode validação.
- **Spells/SpellRuntime.cs** — perfis explícitos de disponibilidade, tipos de
  efeito/entrega, `SpellCastRequest`, `SpellCastResult`, tickets e sessão por
  conjurador. Validação e confirmação de custo não dependem de Unity/VFX.
- **GameIntegration/SpellEffectExecutor.cs** — adaptador nativo dos efeitos
  reutilizáveis; confirma perda/ganho de HP e aplicação de atributos temporários.
- **GameIntegration/WeaponHitObserver.cs** — Harmony em `Actor.getHit`,
  `Projectile.start` e `checkHitForUnit`, com atribuição de XP por dano
  confirmado e identidade da arma capturada no lançamento. Não substitui o
  método original; desinstala somente seus patches ao encerrar o mod.
- **GameIntegration/ModTraits.cs** — 8 escolas, classes, aura, maestria e tiers como ActorTrait.
- **GameIntegration/ActorAbilities.cs** — estados por Actor, magias conhecidas, cooldown, ataque e treino.
- **GameIntegration/CombatBridge.cs** — dano pelo `Actor.getHit` de sete parâmetros,
  com atacante e validação da redução real de HP; cura por `restoreHealth`.
  Não altera HP diretamente nem considera a simples chamada da API como sucesso.
- **GameIntegration/ActorAbilities.cs — conjuração** — validação, um projétil em
  andamento por conjurador, impacto, confirmação de dano e só então mana/recarga.
  Atualização de combate segue `Actor.attack_target` sem depender do ataque corpo a corpo.
- **GameIntegration/GameCompat.cs** — compatibilidade de reflexão para mudanças de propriedades na 0.51.2.
- **GameIntegration/GameWeaponDetector.cs** — tenta reconhecer arma real do ator em lotes.
- **VisualEffects/ScreenEffects.cs** — overlay de UI para isolar problemas de câmera.
  Também oferece trilha de projétil pela câmera real do mapa, separada do dano.
- **VisualEffects/CombatVisualController.cs** — SpriteRenderer em espaço de mundo.
- **UI/CharacterPanels.cs** — abas nativas na ficha do personagem, usando
  `UnitWindow`, `WindowMetaTab` e `SelectedUnit.unit`; atualização visual só
  enquanto uma aba está aberta. A inspeção consulta progresso sem inicializar
  poderes ou conceder XP. O painel de diagnóstico F6 permanece separado.
- **Martial/TechniqueCatalog.cs** — 16 técnicas, das quais quatro básicas
  têm executor; as outras permanecem planejadas.
- **Martial/TechniqueProgression.cs** — conhecimento, origem e nível I–III
  da técnica, separados do grau da arma e da aura; confirmações de usos e recarga.
- **Learning/LearningRules.cs** — regras puras de vocação estável, estudo,
  prática, limites, origem e deduplicação de XP por ataque.
- **GameIntegration/AutonomousLearningAgent.cs** — observa até 24 atores da
  lista nativa de vivos por lote de 0,5 s; usa tempo do mundo e pausa real.
  Mudança de mundo limpa o runtime em memória. Desativar/religar não acumula
  sessões no intervalo em que a opção ficou desligada.
- **Core/WeaponIdentity.cs** — classificação dos IDs de armas; a integração
  obtém o Item real por `Actor.getWeapon()` e o asset por `Item.getAsset()`.
- **Core/ArcaneIdentities.cs** — patronos e origens, bônus e currículos por
  ID estável, compartilhados por aprendizado, mana, registro nativo e ficha.
- **Persistence/ProgressionCodec.cs** — snapshot simples versionado, limites,
  validação atômica e restauração de conhecimento/recursos/recargas, sem Unity.
- **GameIntegration/ActorProgressionPersistence.cs** — prefixo Harmony antes
  de `Actor.prepareForSave`, gravando no campo personalizado do próprio Actor.
- **tests/LearningRulesTests.cs / Phase2RulesTests.cs / NaturalMagicTests.cs /
  IdentityPersistenceTests.cs** — 99 cenários de
  aprendizado, atributos, resumo, técnicas e transação de conjuração, sem
  executar WorldBox; `scripts/test-learning.ps1` reutiliza o compilador do NML.
- **Locales/** — traduções de nomes de traços.

**Evolução recomendada**: extrair `SpellRuntime`/`SpellEffect` em interfaces, resolver posições pela API
estável encontrada no assembly, armazenar estado de personagens com IDs e testar carregamento de saves.
Manter tratamento separado de efeitos (combate) e animações (visuais), para não consumir mana em falha gráfica.

## Decisões e riscos

- A criação de `ActorTrait` depende da API da versão alvo; exemplos antigos podem não compilar.
- `OnGUI` é uma forma simples de provar visibilidade, mas não deve ser o renderer final de milhares de efeitos.
- Os círculos 0 e 1 têm efeitos genéricos limitados, não todas as regras individuais do grimório.
- As outras magias estão planejadas, mas não precisam nem devem consumir mana sem runtime real.
- Persistência básica implementada e testada após reiniciar o WorldBox:
  28 Actors restaurados sem diferenças nos campos comparados; técnica conferida
  na ficha. Aceite nativo de recargas, buffs e valor imediato de aura, herança
  entre gerações e dados institucionais continuam pendentes.
