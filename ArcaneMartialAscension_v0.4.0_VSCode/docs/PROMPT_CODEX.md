# Prompt para continuar o projeto no Codex

Abra a raiz do projeto `ArcaneMartialAscension_v0.4.0_VSCode` no VS Code e peça:

> Você é responsável pela engenharia do mod Arcane & Martial Ascension, para WorldBox 0.51.2,
> com NeoModLoader 1.2.1.1. Leia `AGENTS.md`, `README.md`, `docs/ARQUITETURA.md`,
> `docs/ROADMAP.md` e `docs/TESTES.md` antes de alterar arquivos.
>
> Primeiro confirme se o WorldBox está instalado e localize as DLLs de API sem copiá-las
> para o projeto. Execute a validação e compile o C# contra as DLLs reais. Corrija todos os erros
> de compilação antes de adicionar mecânicas. Depois instale os fontes em `Mods` com o script
> e examine os logs. Se tiver acesso autorizado à interface gráfica, teste F5 (círculo),
> F7 (ator), F8 (Centelha Rubra real), F9 (maestria), F10 (aura). Não afirme que algo funciona
> se não pôde executar o teste. Não implemente todas as 160 magias de uma vez: construa um
> executor reutilizável e valide cada família de efeitos.
>
> Conserve IDs existentes de magias/traços, evite mana gasta em efeito não aplicado e faça
> mudanças pequenas e verificáveis. Relate separadamente o que compilou, o que foi testado
> dentro do jogo e o que permanece pendente.
