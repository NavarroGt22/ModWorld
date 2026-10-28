# Arquitetura v0.4.0 Alpha

**Entrada**: `Code/Main.cs`, classe `Main : BasicMod<Main>`.
O NML lê `mod.json`, compila a pasta `Code` e cria a instância no Unity.

- **Core/** — `ArcaneProgression`, `CharacterProgression`: mana/aura, círculos, XP de armas.
- **Spells/SpellCatalog.cs** — 160 feitiços embutidos em C#, com IDs estáveis. O JSON é documentação,
  não participa do carregamento. Para editar spells, sincronize os dois por ID e rode validação.
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
- **Martial/TechniqueCatalog.cs** — prévia de técnicas por arma e requisito
  de nível; não representa técnicas já executáveis em combate.
- **Learning/LearningRules.cs** — regras puras de vocação estável, estudo,
  prática, limites, origem e deduplicação de XP por ataque.
- **GameIntegration/AutonomousLearningAgent.cs** — observa até 24 atores da
  lista nativa de vivos por lote de 0,5 s; usa tempo do mundo e pausa real.
  Mudança de mundo limpa o runtime em memória. Desativar/religar não acumula
  sessões no intervalo em que a opção ficou desligada.
- **Core/WeaponIdentity.cs** — classificação dos IDs de armas; a integração
  obtém o Item real por `Actor.getWeapon()` e o asset por `Item.getAsset()`.
- **tests/LearningRulesTests.cs** — 24 cenários das regras sem executar WorldBox;
  `scripts/test-learning.ps1` reutiliza o compilador do NML.
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
