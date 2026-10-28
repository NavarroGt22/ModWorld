# Instruções para o Codex — Arcane & Martial Ascension

- Alvo: WorldBox 0.51.2 (Windows/Steam), NeoModLoader 1.2.1.1.
- Mod carregado a partir de `ArcaneMartialAscension/`, entrada `Code/Main.cs` / `mod.json`.
- Não confundir `ArcaneMartialAscension.Dev.csproj` (ferramenta para VS Code) com o mod instalado.
- Não prometer testes concluídos sem executar compilação contra DLLs reais e testar WorldBox.
- Mudanças em API WorldBox devem verificar assinaturas nas DLLs da build do usuário.
- Manter progresso incremental com logs de diagnóstico; não engolir exceções críticas silenciosamente.
- As 160 magias estão em `SpellCatalog.cs`; JSON de Config é espelho descritivo, não lido em runtime.
- Evitar gastar mana quando o efeito concreto falha ou é inexistente.
- Maestria por arma e magia devem pertencer ao Actor correto; no futuro serializar progressão.
- Rodar `scripts/validate.ps1` após mexer no catálogo e `scripts/build.ps1` quando DLLs locais disponíveis.
- Nunca incluir DLLs proprietárias do WorldBox no repositório ou no ZIP.
- Testar em mundo descartável e fazer backup antes de alterar inventário, HP ou traits.
- Prioridades: 1) compilação, 2) registro de traços, 3) F5, 4) F7/F8, 5) maestria em ataques, 6) persistência, 7) IA.
