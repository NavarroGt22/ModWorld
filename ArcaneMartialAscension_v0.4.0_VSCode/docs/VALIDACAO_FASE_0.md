# Validação incremental da Fase 0 — 27/10/2026

## Ambiente observado

- Windows/Steam, WorldBox 0.51.2, build Steam `19962337`.
- Unity `2022.3.60f1`; NeoModLoader `1.2.1.1`.
- Instalação: `C:\Program Files (x86)\Steam\steamapps\common\worldbox`.
- .NET runtime `8.0.11`, sem SDK instalado.
- Compilação local: Roslyn do próprio NML, `Microsoft.CodeAnalysis.CSharp 4.9.0.0`,
  em Windows PowerShell, contra o assembly publicizado da instalação.
- Nenhuma DLL proprietária adicionada ao repositório ou à pasta instalável.

## Falhas encontradas e correções

O `Player.log` anterior registrava CS1503 e CS0117 porque `WeaponType` resolvia
para o enum global do WorldBox (`Melee`, `Range`), em vez do enum do AMA
(`Sword`, `Spear`, `Bow`, `Axe`). `Main`, `ActorAbilities` e `GameWeaponDetector`
agora usam o alias distinto `MartialWeaponType`.

`validate.ps1` não era interpretado pelo PowerShell: a interpolação `$c:` foi
corrigida para `${c}:`. O build agora detecta SDKs, e não apenas a existência
do executável `dotnet`, e usa o compilador do NML quando o SDK está ausente.

O instalador passou a recusar atualização com WorldBox aberto e verifica o
caminho absoluto antes de remover a cópia anterior do mod.

## Verificações executadas

1. `scripts/validate.ps1`: passou; 160 magias, 8 escolas, IDs sincronizados e 16 ícones.
2. `scripts/build.ps1`: passou; 11 fontes C#, zero erros e zero avisos.
3. Análise sintática de todos os scripts PowerShell e `git diff --check`: passaram.
4. Instalação dos fontes e abertura real do WorldBox: o NML compilou o mod,
   chamou `OnModLoad` e exibiu o painel com `Tracos OK` e catálogo 160.
5. F5 com o mapa visível: círculo laranja do overlay com rótulo `VFX GUI TESTE`
   observado. Isso valida apenas o diagnóstico em tela, sem comprovar as
   coordenadas de atores ou os círculos de conjuração no mundo.

Trechos da nova execução em `Player.log`:

```text
[NML]: [Arcane & Martial Ascension]: OnLoad
[NML]: [Arcane & Martial Ascension]: AMA v0.4.0: 160 embedded spells, four weapon mastery paths, registered traits
[NML]: [Arcane & Martial Ascension]: Loaded
[NML]: [Arcane & Martial Ascension]: AMA v0.4.0: VFX screen fallback ready; F5 test, F7 select, F8 cast, F9 weapon, F10 aura
```

Antes da instalação e dos testes, foram criados backups em:

- Saves/autosaves e log anterior: `C:\Users\rn558\Documents\AMA_TestBackups\20261027_212917`.
- Mod anterior: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261027_212918`.

## APIs confirmadas nos metadados locais

A skill `reverse-engineer-anything` e o REA inspecionaram as DLLs sem executá-las.
Estas observações orientam as próximas correções; os adaptadores atuais ainda
não foram substituídos por chamadas diretas nesta revisão.

- `AttackAction.Invoke`: retorna `bool`, recebe dois `BaseSimObject` e um `WorldTile`.
- `Actor.getHit`: retorna `void` e tem sete parâmetros; a busca atual por
  sobrecargas de um parâmetro não alcança esse método.
- `BaseSimObject`: contém `current_position`, `current_tile` e `cur_transform_position`.
- `Actor`: contém `equipment`, `getName()`, `restoreHealth(int)` e `removeTrait(string)`.
- `ActorTrait`: contém `rate_birth`; a tentativa atual de escrever `birth` não encontra esse campo.
- `WeaponType` global: contém `Melee` e `Range`, sem os quatro estilos do AMA.

Identidade do assembly publicizado: SHA-256
`8c54a0b93a3b929834fb426fce8ce66344444e7709484956ddf04267de718234`,
MVID `eaf20368-35e7-4bb6-b082-5477d61019a6`.

Identidade do NeoModLoader: SHA-256
`1dfa185fb9632877698332fd4f8949f14adae30bfbb1941b7b87e81e9f69665c`.

## Limites

O carregamento e a compilação passaram. Isso não valida os callbacks durante
combate, as 160 mecânicas, persistência, IA ou desempenho em batalhas.
O jogo reabriu automaticamente o `save3`; os testes desta revisão se limitaram
ao carregamento e ao overlay F5, sem conjuração manual ou treino. O detector de
armas experimental já existente permanece habilitado por padrão; a revisão
não promete ausência de alterações automáticas em unidades na sessão.
Saves e autosaves anteriores estão no backup registrado acima. Para os testes
de combate seguintes é necessário abrir explicitamente um mundo descartável.
O dano atual ainda usa adaptação experimental e fallback não letal;
a Fase 1 exige validar dano, alvo, mana, recarga e posição no mundo.
