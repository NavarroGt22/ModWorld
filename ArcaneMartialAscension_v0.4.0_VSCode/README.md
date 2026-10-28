# Arcane & Martial Ascension — Projeto v0.4.0 ALPHA (VS Code)

Projeto **editável** para WorldBox **0.51.2** + NeoModLoader **1.2.1.1** no Windows.
Inclui os fontes da build alpha v0.4.0 anterior, mais workspace do VS Code,
`.csproj` para IntelliSense/compilação local, scripts de instalação, validação e logs.

> **Não é uma versão funcionalmente nova da magia.** Código do mod é o mesmo alpha v0.4.0;
> ajustes de tooling não significam que o jogo já foi testado. NÃO validado em WorldBox aqui.

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
Requer **.NET SDK** e restaurar o pacote de referências `net48` via NuGet. A DLL resultante
`ArcaneMartialAscension.Dev.dll` **não deve** ser copiada para `Mods`, para evitar
carregamento duplicado. O NML continua sendo a referência para compilação real.

## O que já existe na v0.4.0

- 160 magias registradas em `Code/Spells/SpellCatalog.cs`, 8 escolas, círculos 0–9;
- Protótipos de mecânicas simples de magias dos círculos **0 e 1**; as demais estão catalogadas,
  **não implementadas**. A interface sinaliza "MECANICA PENDENTE";
- Maestria individual para espada, lança, arco, machado (níveis 0–7), XP e traços;
- Tela de seleção e aprendizado, F5 (VFX de diagnóstico), F7, F8, F9, F10;
- Registros de traços e tentativas de integração com combates reais via APIs/reflection;
- Recursos de RPG atualmente mantidos na memória: **não há persistência completa**.

Problemas a verificar: compilação com as DLLs reais de 0.51.2, detecção de equipamento,
renderização dos círculos, aplicação de dano, IA e serialização da progressão.

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
