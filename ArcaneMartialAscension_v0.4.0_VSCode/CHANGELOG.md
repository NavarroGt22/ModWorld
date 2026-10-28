# Changelog

## Pactos, linhagens e base de persistência — 28/10/2026

- Quatro patronos e quatro origens principais com referência em D&D,
  atributos, mana e currículos próprios; origens anteriores preservadas.
- Dez traços de identidade no grupo Pactos e linhagens; 59 traços BR/EN.
- Ficha com Pacto / linhagem e bônus; aliases dos dois patronos anteriores migrados.
- Snapshot versionado por Actor no save nativo, antes de `prepareForSave`;
  magias, XP, técnicas, recursos, recargas, buffs e aprendizado parcial.
- Restauração atômica, limites de payload, isolamento por Actor e preservação
  de dados incompatíveis. Uso da API pública `Actor.getData()` verificado.
- 99 cenários aprovados e 27 fontes compiladas sem avisos. No jogo,
  formação e repertórios observados para Arquifada/Grande Antigo e
  Dracônica/Selvagem. Mundo salvo no slot 4 e reaberto após encerrar o processo:
  28 Actors restaurados, sem diferenças em magias, XP de espada, mana, patrono
  e origem. Ficha confirmou Aprendiz 1/7 e Corte preciso I; sem exceções na sessão.
- Recargas/buffs ativos e valor imediato de aura ainda precisam de aceite nativo;
  herança entre gerações e conhecimento institucional continuam pendentes.

## Formação natural e primeira demonstração visual — 28/10/2026

- Vocação estável para Mago/Bruxo/Feiticeiro, com estudo, ritual de pacto
  e despertar gradual de origem. Currículos iniciais e experiência próprios.
- Magias registram a tradição pela qual foram aprendidas; conjurar não
  transforma a experiência de bruxos/feiticeiros em experiência de Mago.
- Removida a configuração silenciosa do campo legado `birth`; traços de
  progressão são atribuídos pelo observador e pelo equipamento, com campos
  nativos de nascimento/crescimento/herança desativados.
- Seleção automática de magia extraída para regras testáveis; 74 cenários
  aprovados, incluindo formação/aprendizado/conjuração dos três caminhos.
- No WorldBox: formação e aprendizado dos três caminhos; conjuração
  automática com dano por Magos, Bruxos e Feiticeiro, sem F8.
- Cortes direcionais, rastro/impacto de projéteis e feixe elétrico animado.
  Reuso de sprites e tempo de simulação também no visual de apoio.
- Pactos completos, herança, persistência e animação corporal continuam pendentes.

## Primeira entrega da Fase 2 — 28/10/2026

- Lista de técnicas conhecidas junto dos status da classe, com ícone, nome
  e nível I–III. Conteúdo futuro separado em Próximos treinos.
- Quatro técnicas básicas aprendidas por treino; uso com arma correta,
  recarga própria, dano confirmado e evolução por usos bem-sucedidos.
- XP marcial observado após dano nativo; removido XP por tentativa e dano
  extra genérico. Flechas registram a arma no lançamento.
- Pedidos, resultados, tickets, validação e executor genérico de magias.
- 11 perfis explícitos ativos; mecânicas parciais sinalizadas. Efeitos sem
  implementação própria permanecem indisponíveis, sem custo.
- Relógio de simulação para mana/recarga/bônus e preservação de traços permanentes.
- Diagnóstico F6 com alvo/resultado/técnica. 22 fontes compilados sem avisos;
  58 cenários de regras aprovados. Aceite completo da fase continua pendente.
- No WorldBox: observador registrado, aprendizado manual de Corte preciso I
  e conjuração manual de Centelha Rubra (HP 73→66, mana -3, recarga registrada 2 s).
- F7/F8 e VFX usam coordenadas lógicas estáveis; seleção de alvo não depende
  de transform de renderização atualizado no zoom do mapa.

## Atributos e formação do personagem — 28/10/2026

- Referência visual: prints fornecidas de Combat Gods; composição própria
  com ícones existentes do AMA, cabeçalhos e linhas alternadas.
- Uma área do Grimório reúne tradição, círculo, mana e magias aprendidas;
  escolas mostram seu maior círculo aprendido sem confundi-lo com o filtro.
- Perfis distintos de atributos para três tradições, oito escolas e quatro armas.
- Sete graus de maestria por arma, com progressão de atributos própria;
  apenas o grau atual da arma equipada fica ativo junto da disciplina.
- Grupo de graus no editor e traduções de nomes/descrições para os 49 traços.
- Catálogo de atributos compartilhado pelo registro nativo e pela interface.
- Corrigida falha silenciosa na gravação de atributos: inicialização de
  `BaseStats`, indexador nativo e conferência dos valores, após erro observado no jogo.
- Graus e buffs não podem surgir por concessão aleatória ou nascimento.
- Remoção imediata do `DragOrderElement` apenas nos botões clonados, evitando
  a corrida com o organizador nativo observada ao abrir a ficha.
- 18 fontes compiladas sem avisos/erros e 31 cenários de regras aprovados.
- No WorldBox: registro/leitura dos 49 traços confirmado, abas abertas e
  alternadas sem novo erro, e um aprendizado espontâneo de Centelha Rubra observado.
- Técnicas especiais, balanceamento de combate e persistência seguem em validação/desenvolvimento.

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
