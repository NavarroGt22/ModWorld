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
     58 cenários de aprendizado, atributos, técnicas e conjuração, sem executar a simulação do WorldBox.
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

- **Classe marcial** (espada): espada, lança, arco e machado; nível 0–7,
  XP, próximo grau, arma equipada e técnicas conhecidas na mesma área de status.
  A lista usa ícone, nome e nível I–III; clicar mostra os detalhes e a origem.
  Os sete graus têm atributos próprios por arma; só o grau atual se soma à
  disciplina equipada. Faixas e linhas de informação seguem o estilo nativo.
  Uma técnica básica por arma é aprendida pelo treino no nível 1. As técnicas
  avançadas aparecem como **planejadas**, abaixo, em Próximos treinos.
- **Grimório** (mago): formação arcana reunida numa seção com tradição,
  círculo mágico, mana e quantidade de magias aprendidas. Cada escola mostra
  seu maior círculo aprendido e total conhecido; afinidade sem magia fica explícita.
  O filtro do catálogo 0–9 fica separado do círculo alcançado pelo personagem.
  Atributos distintos para cada tradição e escola também aparecem no painel.
  **Aprender** usa os requisitos existentes; **Preparar · F8** escolhe a magia
  para conjurar sobre um alvo no mapa com F8.
  A magia preparada também tem preferência no combate automático, quando há
  inimigo válido, alcance, mana e recarga disponíveis. O jogo precisa estar rodando.

As abas seguem o habitante inspecionado pelo jogo. Abrir ou alternar as abas
não concede XP, traços ou habilidades. O painel antigo de diagnóstico começa
oculto; **F6** mostra/oculta. A progressão continua apenas em memória.
Detalhes de implementação e testes: `docs/INTERFACE_PERSONAGEM.md`.

- 160 magias registradas em `Code/Spells/SpellCatalog.cs`, 8 escolas, círculos 0–9;
- Motor genérico de conjuração com 11 perfis ativos dos círculos **0 e 1**;
  3 têm mecânicas parciais sinalizadas. As demais magias continuam catalogadas;
- Maestria individual para espada, lança, arco, machado (níveis 0–7), XP e traços;
- Tela de seleção e aprendizado, F5 (VFX de diagnóstico), F7, F8, F9, F10;
- Registros de traços e tentativas de integração com combates reais via APIs/reflection;
- Persistência versionada por Actor no save nativo; 28 personagens restaurados
  após reiniciar o jogo, sem diferenças nos campos comparados. Herança entre
  gerações e instituições pendentes.

Problemas a verificar: seleção e posição de unidades, detecção de equipamento,
círculos de conjuração no mundo, aplicação de dano, IA e serialização da progressão.
Compilação, carregamento e círculo GUI do F5 passaram nesta revisão.

### Revisão de conjuração em validação

Perfis de projétil acompanham o alvo; perfis instantâneos aplicam o efeito
diretamente. O dano usa a API nativa e confirma HP antes de cobrar mana/recarga.
O F8 usa a câmera do mapa e exibe mensagens de resultado mesmo com F6 oculto.
O automático acompanha o alvo de combate do personagem e considera suas magias
aprendidas, em vez de aguardar somente um ataque comum.
Essa revisão ainda não representa os efeitos exclusivos das 160 magias.
No teste da Fase 2, uma Centelha Rubra manual reduziu HP de 73 para 66,
consumiu 3 de mana e registrou recarga de 2 s. Na expansão posterior, os três
caminhos conjuraram automaticamente contra ursos; testes de carga e aceites
completos das mecânicas continuam pendentes.
Teste e limitações: `docs/VALIDACAO_CONJURACAO.md`.

### Protótipo de aprendizado autônomo

- Habitantes sapientes adultos podem desenvolver três tradições. A vocação
  estável por ID usa faixas de 7% Mago, 2% Bruxo e 3% Feiticeiro; não são
  percentuais garantidos em uma população pequena.
- Mago inicia os estudos; Feiticeiro desperta uma origem após 24 unidades
  observadas em paz; Bruxo conclui um ritual de pacto após 64. O observador
  respeita tradições já atribuídas. Inspecionar ou inicializar estado não concede poderes.
- Cada caminho aprende quatro magias iniciais dos círculos 0–1, com requisitos
  de círculo e trabalho gradual. O repertório depende do patrono ou da origem;
  Magos mantêm o currículo inicial de Evocação.
- Aprendizado e conjuração dão XP à tradição correta; pacto, despertar,
  estudo próprio e intervenção do jogador têm origens distintas. Não há
  negociação com patronos, obrigações ou herança de linhagens nesta entrega.
- Habitantes adultos com espada, lança, arco ou machado podem praticar em paz.
  Acertos nativos com perda real de vida concedem XP, uma vez por frame/ator.
  Flechas conservam a identidade da arma usada no lançamento.
- Trocar ou retirar a arma muda os bônus ativos e não transfere XP entre armas.
  Técnicas avançadas planejadas não passam a funcionar por atingir um nível.
- Estudo e prática são passivos neste protótipo: ainda não há busca de
  professores, academias, animação de treino ou agenda social.
- **F6 → IA: aprender magias e treinar armas** ativa/desativa o protótipo.
  As abas mostram a última atividade e a origem de magias aprendidas.
- **Persistência básica testada:** XP, magias, técnicas, mana, aura, pacto,
  linhagem e trabalho parcial integram o save do próprio Actor. Reiniciar e
  reabrir confirmou os dados comparados de 28 Actors e Corte preciso I na ficha.
  Escopo do aceite e limites: `docs/PACTOS_LINHAGENS_E_PERSISTENCIA.md`.

Validação atual: 99 cenários de regras aprovados. Em mundo descartável,
as três tradições se formaram e aprenderam sem ensino manual; magos, bruxos e
feiticeiro conjuraram automaticamente contra ursos, com dano confirmado.
Prática armada e aceites completos dos repertórios continuam pendentes.
Evidências e limites: `docs/FORMACAO_NATURAL_E_FX.md`.
Detalhes: `docs/VALIDACAO_APRENDIZADO.md`.

### Fase 2 iniciada

Pedidos e resultados de magia, reserva por conjurador, validações de alvo e
executor de efeitos foram separados dos visuais. Mana e recarga só entram
após um efeito válido. O tempo de simulação controla mana, recarga e bônus
temporários. O F6 também mostra alvo, resultado recente e técnica utilizada.
Escudo de absorção, invocação, teleporte e mecânicas específicas de controle
aguardam implementação própria. Escopo e testes: `docs/FASE_2.md`.

### Primeira demonstração visual da Fase 3

Corte direcional, projétil com rastro/impacto e feixe elétrico segmentado.
Há desenho de apoio na interface, reutilização de até 48 objetos de sprite
e relógio de simulação para os efeitos. Isso ainda não anima o corpo dos
personagens nem implementa combos, bloqueios ou esquivas.

### Pactos, linhagens e Fase 5

Referência nos arquétipos de D&D: Corruptor, Arquifada, Celestial e Grande
Antigo; origens Dracônica, Selvagem, Aberrante e Mecânica. Atributos, mana e
currículos são adaptações próprias para WorldBox. As origens anteriores
Elemental/Feérica e os IDs legados de patrono continuam reconhecidos.
A ficha mostra Pacto / linhagem e seus bônus. Poderes exclusivos completos
e herança biológica ainda não estão implementados.

A persistência usa `ama.progression.v1` nos dados nativos de cada Actor,
com esquema versionado e validação atômica. Dados inválidos ou de versão
não suportada são preservados, sem sobrescrever o progresso original.
O mundo de teste foi salvo no slot 4 e reaberto após encerrar o processo.
Magias, XP de espada, mana, patrono e origem coincidiram nos 28 Actors comparados;
recargas/buffs ativos e o valor imediato de aura ainda precisam de aceite nativo.

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
