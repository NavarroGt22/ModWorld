# Arcane & Martial Ascension — Projeto v0.4.0 ALPHA (VS Code)

Projeto **editável** para WorldBox **0.51.2** + NeoModLoader **1.2.1.1** no Windows.
Inclui os fontes da build alpha v0.4.0 anterior, mais workspace do VS Code,
`.csproj` para IntelliSense/compilação local, scripts de instalação, validação e logs.

> **Alpha em validação incremental.** Nesta revisão, o conflito de `WeaponType` foi corrigido,
> a compilação passou com as DLLs locais e o carregamento/registro de traços foi observado
> no WorldBox 0.51.2 + NeoModLoader 1.2.1.1. As mecânicas restantes continuam experimentais.
> Consulte `docs/VALIDACAO_FASE_0.md` para evidências e limites dos testes.

## Começar no VS Code (Windows)

1. **Extraia** o ZIP em uma pasta de projetos, por exemplo `Documentos\Projetos\ArcaneMartialAscension_v0.4.0_VSCode`.
   Não copie a pasta inteira do projeto para `Mods`.
2. Abra **VS Code → Arquivo → Abrir Workspace do Arquivo... → `AMA.code-workspace`**.
   Se preferir, use **Arquivo → Abrir Pasta** nesta mesma pasta.
3. Instale a extensão **C#** (Microsoft). Para IntelliSense avançado use **C# Dev Kit**.
4. Abra `ArcaneMartialAscension/Code/Main.cs`. Os arquivos principais estão em `Code/`.
5. **Terminal → Executar Tarefa...**:
   - `AMA: 1 - Validar catálogo e estrutura`: testa JSONs, IDs, ícones e contagem das 160 magias;
   - `AMA: 2 - Compilar C# (DLLs locais)`: exige .NET SDK e DLLs na pasta do seu WorldBox;
   - `AMA: 3 - Instalar fontes no WorldBox`: faz backup do mod anterior **fora** de `Mods`, copia a pasta instalável;
   - `AMA: 4 - Ver erros do Player.log`: procura erros e mensagens do AMA nos logs.
   - `AMA: 5 - Testar aprendizado e maestrias`: compila com DLLs locais e executa
     24 cenários das regras, sem executar a simulação do WorldBox.
6. Feche o WorldBox antes de instalar, inicie o jogo em **Modo Experimental** e use um **mundo de teste**.

### Jogo instalado fora do caminho padrão?

O script detecta `C:\Program Files (x86)\Steam\steamapps\common\worldbox` por padrão.
Se estiver em outra unidade, abra o PowerShell no VS Code:

```powershell
.\scripts\install.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\worldbox'
.\scripts\build.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\worldbox'
```

Ou configure a variável de ambiente `WORLD_BOX_PATH` com a pasta onde está `worldbox.exe`.

### Compilação versus instalação (importante)

O **NeoModLoader compila `Code/*.cs` ao carregar o mod**. Portanto, para editar e testar
você **não precisa compilar manualmente pelo `dotnet`**: instale os fontes pela tarefa 3,
reinicie o jogo e leia os logs. <https://github.com/WorldBoxOpenMods/ModLoader>

O projeto `ArcaneMartialAscension.Dev.csproj` é **opcional**, destinado a facilitar o editor
e apontar erros C# antes de abrir o jogo. Ele referencia DLLs do WorldBox **instaladas na sua
máquina** (`Assembly-CSharp`, módulos Unity, NeoModLoader), que não estão neste ZIP.
O caminho pelo `.csproj` requer **.NET SDK** e restauração das referências `net48` via NuGet.
Se o SDK estiver ausente, `scripts/build.ps1` usa `scripts/build-nml.ps1` com o Roslyn
do NML instalado e Windows PowerShell 5.1; esse caminho não baixa dependências nem copia
DLLs do jogo. A saída fica em `bin/Debug/nml/`. A DLL resultante
`ArcaneMartialAscension.Dev.dll` **não deve** ser copiada para `Mods`, para evitar
carregamento duplicado. O NML continua sendo a referência para compilação real.

## O que já existe na v0.4.0

### Abas na ficha do personagem

Abra a ficha de um habitante. Dois novos ícones na coluna lateral dão acesso a:

- **Maestrias** (espada): espada, lança, arco e machado; nível 0–7, título,
  XP, barra até o próximo nível e prévia das técnicas da arma escolhida.
  Técnicas sem executor de combate aparecem explicitamente como **planejadas**.
- **Grimório** (mago): oito escolas, filtro por círculo 0–9, magias aprendidas,
  mana, recarga, descrição e indicação de protótipo/planejamento.
  **Aprender** usa os requisitos existentes; **Preparar · F8** escolhe a magia
  para conjurar sobre um alvo no mapa com F8.
  A magia preparada também tem preferência no combate automático, quando há
  inimigo válido, alcance, mana e recarga disponíveis. O jogo precisa estar rodando.

As abas seguem o habitante inspecionado pelo jogo. Abrir ou alternar as abas
não concede XP, traços ou habilidades. O painel antigo de diagnóstico começa
oculto; **F6** mostra/oculta. A progressão continua apenas em memória.
Detalhes de implementação e testes: `docs/INTERFACE_PERSONAGEM.md`.

- 160 magias registradas em `Code/Spells/SpellCatalog.cs`, 8 escolas, círculos 0–9;
- Protótipos de mecânicas simples de magias dos círculos **0 e 1**; as demais estão catalogadas,
  **não implementadas**. A interface sinaliza "MECANICA PENDENTE";
- Maestria individual para espada, lança, arco, machado (níveis 0–7), XP e traços;
- Tela de seleção e aprendizado, F5 (VFX de diagnóstico), F7, F8, F9, F10;
- Registros de traços e tentativas de integração com combates reais via APIs/reflection;
- Recursos de RPG atualmente mantidos na memória: **não há persistência completa**.

Problemas a verificar: seleção e posição de unidades, detecção de equipamento,
círculos de conjuração no mundo, aplicação de dano, IA e serialização da progressão.
Compilação, carregamento e círculo GUI do F5 passaram nesta revisão.

### Revisão de conjuração em validação

Magias ofensivas iniciais agora lançam um projétil até o alvo, aplicam dano
pela API nativa e verificam o HP antes de cobrar mana ou iniciar recarga.
O F8 usa a câmera do mapa e exibe mensagens de resultado mesmo com F6 oculto.
O automático acompanha o alvo de combate do personagem e considera suas magias
aprendidas, em vez de aguardar somente um ataque comum.
Essa revisão ainda não representa os efeitos exclusivos das 160 magias.
Teste e limitações: `docs/VALIDACAO_CONJURACAO.md`.

### Protótipo de aprendizado autônomo

- Habitantes sapientes adultos com vocação arcana estudam quatro magias de
  Evocação dos círculos 0 e 1. A vocação inicial é determinada pelo ID do
  personagem, com configuração de 12%; magos existentes também podem estudar.
- O aprendizado exige sessões observadas de estudo e registra **estudo próprio**
  como origem. Não são mais atribuídos uma magia gratuita, pacto fictício ou
  origem de feiticeiro ao inicializar qualquer personagem.
- Habitantes adultos com espada, lança, arco ou machado podem praticar em paz.
  Ataques válidos também concedem XP, uma vez por frame de ataque do personagem.
- Trocar ou retirar a arma muda os bônus ativos e não transfere XP entre armas.
  Técnicas ainda planejadas não passam a funcionar por atingir um nível.
- Estudo e prática são passivos neste protótipo: ainda não há busca de
  professores, academias, animação de treino ou agenda social.
- **F6 → IA: aprender magias e treinar armas** ativa/desativa o protótipo.
  As abas mostram a última atividade e a origem de magias aprendidas.
- **Progresso ainda não é salvo:** XP, estudo e conhecimentos permanecem em memória.

Validação: 16 fontes compiladas sem erros/avisos e 24 cenários de regras aprovados.
O aprendizado dentro do jogo ainda precisa de teste em mundo descartável.
Detalhes: `docs/VALIDACAO_APRENDIZADO.md`.

## Localização dos arquivos

```text
ArcaneMartialAscension_v0.4.0_VSCode/
├── AMA.code-workspace
├── ArcaneMartialAscension.Dev.csproj  # Referências ao seu jogo (opcional)
├── ArcaneMartialAscension/             # ESTA é a pasta instalável do mod
│   ├── mod.json
│   ├── Code/
│   │   ├── Main.cs
│   │   ├── Core/
│   │   ├── Spells/SpellCatalog.cs
│   │   ├── GameIntegration/
│   │   └── VisualEffects/
│   ├── Config/
│   ├── GameResources/
│   └── Locales/
├── .vscode/tasks.json
├── scripts/                            # build, install, logs, validate
├── docs/                               # arquitetura, testes e roadmap
└── AGENTS.md                           # instruções de projeto para Codex
```

O NML espera especificamente **`Mods\ArcaneMartialAscension\mod.json`**, não a pasta pai.

## Documentação adicional

- `docs/TESTES.md`: checklist reproduzível de testes no WorldBox.
- `docs/ARQUITETURA.md`: mapa dos subsistemas e limitações técnicas.
- `docs/ROADMAP.md`: próximos passos e critérios de pronto.
- `docs/PROMPT_CODEX.md`: texto pronto para orientar o Codex.
- `ArcaneMartialAscension/README.md`: comportamento e teclas do mod.
