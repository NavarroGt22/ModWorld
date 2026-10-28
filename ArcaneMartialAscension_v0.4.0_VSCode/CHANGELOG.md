# Changelog

## Aprendizado autônomo experimental — 27/10/2026

- Estudo próprio gradual de quatro magias iniciais de Evocação e registro
  de origem por personagem; retirada de poderes de teste concedidos na inicialização.
- Prática da arma equipada e XP de combate sem duplicar callbacks no mesmo frame.
- Detecção nativa do Item e seu asset; bônus ativos apenas da arma equipada.
- Tempo de estudo/prática compartilhado, pausa, interrupções e processamento em lotes.
- Progresso e origem visíveis nas abas; opção de aprendizado em F6.
- Compilação: 16 fontes, zero erros/avisos. Regras: 24 cenários passaram.
- Testes no WorldBox e persistência continuam pendentes. Sem mudança de versão estável.
- Roadmap atualizado com o estado verificado; Fase 1 permanece em validação.

## Interface do personagem — em desenvolvimento

- Duas abas nativas na coluna lateral da ficha: Maestrias e Grimório.
- Nível, XP e progresso independente para as quatro armas; prévia de técnicas
  planejadas e seus requisitos.
- Escolas mágicas, filtro por círculo 0–9, descrição, custo, recarga,
  aprendizado e preparação de magias usando o personagem inspecionado.
- Consultar a interface não inicializa poderes nem altera progressão.
- Painel de diagnóstico oculto por padrão; F6 continua disponível.
- Compilação com DLLs reais: 13 fontes, zero erros e avisos.
- Validação no jogo pendente; ver `docs/INTERFACE_PERSONAGEM.md`.

## Revisão de desenvolvimento da v0.4.0 — 27/10/2026

- Corrigido conflito entre o enum `WeaponType` do WorldBox e as armas do AMA.
- Corrigida interpolação inválida que impedia executar `scripts/validate.ps1`.
- Build detecta ausência de SDK e usa Roslyn do NML instalado via Windows PowerShell.
- Instalação verifica o destino absoluto e recusa atualização com o jogo aberto.
- Confirmados: catálogo estático, compilação com DLLs reais sem erros/avisos,
  carregamento no WorldBox 0.51.2/NML 1.2.1.1 e círculo GUI do F5.
- Dano real, seleção, callbacks de combate e persistência continuam pendentes;
  evidências e limitações em `docs/VALIDACAO_FASE_0.md`.
