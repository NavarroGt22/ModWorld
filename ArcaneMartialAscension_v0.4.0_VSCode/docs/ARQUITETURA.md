# Arquitetura v0.4.0 Alpha

**Entrada**: `Code/Main.cs`, classe `Main : BasicMod<Main>`.
O NML lê `mod.json`, compila a pasta `Code` e cria a instância no Unity.

- **Core/** — `ArcaneProgression`, `CharacterProgression`: mana/aura, círculos, XP de armas.
- **Spells/SpellCatalog.cs** — 160 feitiços embutidos em C#, com IDs estáveis. O JSON é documentação,
  não participa do carregamento. Para editar spells, sincronize os dois por ID e rode validação.
- **GameIntegration/ModTraits.cs** — 8 escolas, classes, aura, maestria e tiers como ActorTrait.
- **GameIntegration/ActorAbilities.cs** — estados por Actor, magias conhecidas, cooldown, ataque e treino.
- **GameIntegration/CombatBridge.cs** — tentativa de APIs de HP/status, fallback reflexivo experimental;
  não garante dano letal nem contagem correta de abates.
- **GameIntegration/GameCompat.cs** — compatibilidade de reflexão para mudanças de propriedades na 0.51.2.
- **GameIntegration/GameWeaponDetector.cs** — tenta reconhecer arma real do ator em lotes.
- **VisualEffects/ScreenEffects.cs** — overlay de UI para isolar problemas de câmera.
- **VisualEffects/CombatVisualController.cs** — SpriteRenderer em espaço de mundo.
- **Locales/** — traduções de nomes de traços.

**Evolução recomendada**: extrair `SpellRuntime`/`SpellEffect` em interfaces, resolver posições pela API
estável encontrada no assembly, armazenar estado de personagens com IDs e testar carregamento de saves.
Manter tratamento separado de efeitos (combate) e animações (visuais), para não consumir mana em falha gráfica.

## Decisões e riscos

- A criação de `ActorTrait` depende da API da versão alvo; exemplos antigos podem não compilar.
- `OnGUI` é uma forma simples de provar visibilidade, mas não deve ser o renderer final de milhares de efeitos.
- Os círculos 0 e 1 têm efeitos genéricos limitados, não todas as regras individuais do grimório.
- As outras magias estão planejadas, mas não precisam nem devem consumir mana sem runtime real.
- A progressão existe em memória; persistência e migração de saves **pendentes**.
