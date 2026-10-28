# Arcane & Martial Ascension

## Roadmap completo de desenvolvimento — WorldBox

**Documento:** planejamento técnico e de produto  
**Base atual:** v0.4.0 Alpha (projeto para VS Code/Codex)  
**Ambiente-alvo:** WorldBox 0.51.2 (Steam/Windows) + NeoModLoader
1.2.1.1  
**Status:** plano de implementação; itens não assinalados ainda precisam
ser validados ou desenvolvidos  
**Objetivo final:** um ecossistema de fantasia em que habitantes
aprendem, treinam, lutam, transmitem conhecimentos e evoluem
autonomamente.

> **Nota de escopo:** o catálogo de 160 magias já está definido no
> projeto v0.4.0, mas isso **não** significa que as 160 magias tenham
> efeitos funcionais. A versão atual é uma base experimental, ainda não
> confirmada em execução no WorldBox do usuário. O roadmap abaixo é a
> proposta de evolução, não uma declaração de funcionalidades prontas.

------------------------------------------------------------------------

# 1. Visão do projeto

Transformar o WorldBox em um mundo de fantasia com progressão emergente:

- **Magos:** aprendem por estudo, pesquisa, professores e grimórios.
- **Feiticeiros:** desenvolvem dons inatos, afinidades e linhagens.
- **Bruxos:** recebem poderes por pactos e relações com patronos.
- **Guerreiros:** treinam armas, posturas, combos, defesa, técnicas
  especiais e aura.
- **Civilizações:** criam instituições que ensinam, preservam e difundem
  conhecimento.
- **IA:** utiliza poderes de acordo com distância, perigo, mana,
  habilidade, objetivo e contexto.
- **Simulação:** a evolução acontece mesmo sem o jogador atribuir
  poderes manualmente.
- **Jogador:** pode observar, configurar regras, inspecionar personagens
  e intervir quando desejar.

## 1.1 Princípios obrigatórios

1.  **Todo poder tem origem:** nascimento, treino, estudo, professor,
    grimório, pacto, linhagem ou evento extraordinário.
2.  **Poder e visual são sistemas separados:** o cálculo de combate deve
    funcionar mesmo se uma animação não for exibida.
3.  **Mana só é consumida em conjuração válida:** falha por magia não
    implementada, alvo inválido ou API incompatível não deve gastar
    recursos.
4.  **Progressão pertence ao personagem:** cada habitante tem seus
    próprios conhecimentos, maestrias e histórico.
5.  **Persistência é essencial:** salvar e carregar não pode apagar a
    progressão.
6.  **Desempenho importa:** efeitos, IA e verificações precisam escalar
    para centenas ou milhares de unidades.
7.  **Compatibilidade é verificável:** nenhuma versão é considerada
    concluída apenas porque o código foi escrito.

------------------------------------------------------------------------

# 2. Situação atual — v0.4.0 Alpha

## 2.1 Elementos já presentes no projeto (implementação preliminar)

- [x] Estrutura de projeto para **VS Code**, com scripts de validação,
  instalação e diagnóstico.
- [x] Catálogo em C# com **160 magias**, distribuídas em **8 escolas** e
  **círculos 0–9**.
- [x] Modelos iniciais de **mana, círculos, magia aprendida e recarga**.
- [x] Modelo de **maestria de espada, lança, arco e machado**, com XP e
  níveis 0–7.
- [x] Protótipos de **traços**, aura, integração com ataques e detecção
  de equipamento.
- [x] Protótipos de **efeitos visuais** em mundo e overlay de
  diagnóstico (F5).
- [x] Controles experimentais para selecionar unidade e executar ações
  (F7–F10).

## 2.2 Pendências conhecidas e não verificadas

- [ ] Compilação real com as DLLs do **WorldBox 0.51.2**.
- [ ] Carregamento no **NeoModLoader 1.2.1.1** sem erros.
- [ ] Círculos mágicos visíveis e posicionados corretamente.
- [ ] Magias com efeitos concretos, dano, cura e aplicação de estados
  confiáveis.
- [ ] XP de maestria contabilizado em ataques reais sem duplicação.
- [ ] IA autônoma de combate e aprendizado.
- [ ] Persistência completa de personagens e migração de saves.
- [ ] Efeitos exclusivos para todas as 160 magias.
- [ ] Testes de desempenho em batalhas grandes.

**Arquivos de referência do projeto VS Code:** `README.md`, `AGENTS.md`,
`docs/ARQUITETURA.md`, `docs/ROADMAP.md`, `docs/TESTES.md`,
`docs/PROMPT_CODEX.md` e `ArcaneMartialAscension/Code/`.

------------------------------------------------------------------------

# 3. Fases do desenvolvimento

## Fase 0 — Preparação e auditoria técnica

**Meta:** ter um ambiente de desenvolvimento reproduzível.  
**Prioridade:** bloqueadora. **Versão:** preparação para v0.4.1.

### Tarefas

- [ ] Abrir o workspace no VS Code e configurar C# e .NET SDK.
- [ ] Localizar `worldbox.exe`, `Assembly-CSharp.dll`, módulos Unity e
  DLLs do NML no computador de teste.
- [ ] Confirmar as assinaturas reais das APIs de `Actor`, `ActorTrait`,
  dano, posição, equipamento e callbacks.
- [ ] Corrigir erros de compilação identificados no log; substituir
  reflexão frágil por API direta quando comprovada.
- [ ] Definir repositório Git, branches, convenções, changelog e tags de
  versão.
- [ ] Automatizar validação estática do catálogo e compilação quando
  possível.
- [ ] Criar um mundo descartável de teste e guardar backups dos saves.
- [ ] Registrar versões exatas do jogo, loader e dependências no
  relatório de teste.

**Entregáveis:** ambiente VS Code funcional, compilação reproduzível,
relatório de APIs confirmadas.  
**Aceite:** o mod compila e carrega sem erros de inicialização na
instalação-alvo.

------------------------------------------------------------------------

## Fase 1 — Fundação de combate e efeitos mágicos

**Versão proposta:** v0.4.1  
**Objetivo:** fazer **uma magia real** funcionar do início ao fim.

### 1.1 Integração com o mundo

- [ ] Obter posição real de cada unidade sem usar cursor como substituto
  silencioso.
- [ ] Selecionar conjurador e alvo com confirmação visual.
- [ ] Validar alvo vivo, facção, distância, linha de visão (quando
  aplicável) e regras de aliados.
- [ ] Criar camada de integração com APIs reais do WorldBox.

### 1.2 Conjuração

- [ ] Implementar fluxo: **solicitação → validação → execução →
  resultado → custo/recarga**.
- [ ] Impedir consumo de mana quando a execução falhar antes da
  aplicação do efeito.
- [ ] Registrar eventos e erros de magia com IDs e informações de
  diagnóstico.
- [ ] Implementar ao menos um projétil real, um impacto e dano
  mensurável.
- [ ] Testar a magia **Centelha Rubra** em dois habitantes de um mundo
  de teste.

### 1.3 Círculos e renderização

- [ ] Corrigir desenho do círculo mágico no mapa.
- [ ] Confirmar camada, escala, câmera, rotação, posição e duração.
- [ ] Manter F5 como diagnóstico de renderização, separado da magia.
- [ ] Garantir que um problema visual não invalide dano já executado.
- [ ] Introduzir configuração de intensidade dos efeitos.

**Entregáveis:** motor mínimo de conjuração, círculo visível, projétil,
impacto, dano, logs.  
**Aceite:** selecionar unidade, conjurar sobre inimigo, observar efeito
e dano; mana e recarga corretas; sem exceções no log.

------------------------------------------------------------------------

## Fase 2 — Motor genérico de magias e maestria confiável

**Versão proposta:** v0.4.2  
**Objetivo:** substituir comportamentos especiais improvisados por
componentes reutilizáveis.

### 2.1 Motor de magias

- [ ] Definir `SpellDefinition`, `SpellCastRequest`, `SpellCastResult` e
  `SpellEffect`.
- [ ] Criar tipos de efeito: projétil, dano direto, área, cura, escudo,
  buff, debuff, invocação e teleporte.
- [ ] Definir validações por efeito e regras de alvo.
- [ ] Separar catálogo, regras, runtime e VFX.
- [ ] Associar a cada magia um estado: **catalogada**, **implementada**,
  **testada**.
- [ ] Implementar todas as magias dos **círculos 0 e 1** com
  comportamento próprio, gradualmente.
- [ ] Manter magias pendentes indisponíveis e sem custo de mana.

### 2.2 Maestria com armas

- [ ] Detectar corretamente espada, lança, arco e machado equipados.
- [ ] Conceder XP por treino, acerto e combate, com prevenção de
  duplicação/exploração.
- [ ] Persistir XP e nível separadamente por arma.
- [ ] Aplicar bônus por nível e desbloqueios apenas quando a arma
  correta estiver equipada.
- [ ] Testar progressão do nível 0 ao 7.
- [ ] Separar domínio da arma, técnicas aprendidas e poder de aura.

### 2.3 Ferramentas

- [ ] Painel de diagnóstico com mana, círculo, magia, arma, XP, alvo e
  erro recente.
- [ ] Testes unitários para custo, cooldown, desbloqueio, XP e regras de
  alvo.

**Entregáveis:** executor genérico, magias iniciais reais, maestria
integrada ao combate.  
**Aceite:** quatro armas progridem independentemente; magia pendente não
gasta mana; efeitos reutilizáveis passam nos testes.

------------------------------------------------------------------------

## Fase 3 — Combate animado de magos e guerreiros

**Versão proposta:** v0.5.0  
**Objetivo:** tornar batalhas visualmente expressivas sem prejudicar a
simulação.

### 3.1 Magos

- [ ] Animações de preparação, gestos e conjuração.
- [ ] Círculos por escola, com símbolos, cores e duração diferenciados.
- [ ] Projéteis animados, explosões, feixes, escudos, auras, teleporte e
  invocações.
- [ ] Efeitos proporcionais ao círculo, sem aumentar automaticamente o
  dano por estética.
- [ ] Interrupção de conjuração, canalização e reação ao impacto.
- [ ] Tratamento de conjuração simultânea por dezenas de magos.

### 3.2 Guerreiros

- [ ] Golpes direcionais, estocadas, cortes amplos e ataques pesados.
- [ ] Bloqueio, esquiva, aparo, contra-ataque e recuperação.
- [ ] Combos de espada, lança, arco e machado com diferenças mecânicas.
- [ ] Trilhas de movimento, ondas de corte e aura para níveis avançados.
- [ ] Técnicas especiais com custos, recargas e condições de ativação.
- [ ] Interrupção e prioridade de animação para evitar ações
  sobrepostas.

### 3.3 Engenharia de VFX

- [ ] Pooling de partículas e objetos.
- [ ] Limite por quadro e por unidade; descarte de efeitos fora da
  câmera.
- [ ] Configuração **baixo / médio / alto / cinematográfico**.
- [ ] Medição de FPS, alocações e tempo de atualização.

**Entregáveis:** biblioteca de animações, estados de combate e níveis de
qualidade.  
**Aceite:** ações visivelmente distintas e coerentes; sem travamentos
relevantes em cenários de teste definidos.

------------------------------------------------------------------------

## Fase 4 — Aprendizado, progressão e origem dos poderes

**Versão proposta:** v0.6.0  
**Objetivo:** nenhum habitante receber uma habilidade avançada “do
nada”.

### 4.1 Atributos de aprendizado

- [ ] Aptidão arcana, afinidade por escola, talento marcial e capacidade
  de controle.
- [ ] Experiência por escola, arma, técnica e classe.
- [ ] Requisitos de mana, conhecimento, nível e condições para cada
  círculo.
- [ ] Raridade configurável de talentos e limites de crescimento.
- [ ] Registro da **origem de cada habilidade**.

### 4.2 Caminhos mágicos

**Mago — conhecimento adquirido** - \[ \] Aprende por estudo,
biblioteca, grimório, academia e professor. - \[ \] Pesquisa novas
magias e exige tempo/recursos conforme dificuldade. - \[ \] Pode
especializar-se em uma ou mais escolas, sujeito a regras de
balanceamento.

**Feiticeiro — poder inato** - \[ \] Recebe predisposições de linhagem e
desperta poderes em eventos ou marcos de crescimento. - \[ \] Evolui por
uso e controle da própria energia. - \[ \] Possui seleção de magias
compatível com sua afinidade e progressão.

**Bruxo — pacto** - \[ \] Obtém poderes por vínculo com patrono. - \[ \]
Tem condições, benefícios, custos e possíveis consequências do pacto. -
\[ \] Pode ampliar repertório ao cumprir obrigações ou fortalecer
vínculo.

### 4.3 Caminho marcial

- [ ] Aprende técnicas com instrutores, treino, duelos e experiência
  real.
- [ ] Pode estudar posturas, defesa e combos específicos de cada arma.
- [ ] Despertar de aura com condições explícitas, sem ser obrigatório
  para mestres de armas.
- [ ] Aprendizado de técnicas raras por observação ou transmissão, com
  limitações.
- [ ] Criar chance de falha e recuperação para treinamentos difíceis.

### 4.4 Rastreabilidade

- [ ] Campos sugeridos: `sourceType`, `sourceId`, `teacherId`,
  `learnedAt`, `method`, `mastery`.
- [ ] Interface “Como aprendeu?” no painel do personagem.
- [ ] Histórico de eventos: **estudou**, **aprendeu**, **despertou**,
  **dominou**, **ensinou**.

**Entregáveis:** sistema de conhecimento, desbloqueios, treinamento,
origem de poderes.  
**Aceite:** personagem inicia sem feitiço avançado, aprende por caminho
válido e mantém registro da origem.

------------------------------------------------------------------------

## Fase 5 — Persistência e evolução entre gerações

**Versão proposta:** v0.6.1  
**Objetivo:** garantir que a progressão continue depois de salvar,
carregar e reiniciar.

### Tarefas

- [ ] Definir ID estável por personagem e política para morte/remoção de
  unidade.
- [ ] Salvar mana, círculos, magias, XP, maestrias, técnicas, pactos,
  professores e histórico essencial.
- [ ] Salvar vínculos institucionais e conhecimento de facções.
- [ ] Testar leitura, escrita, integridade e limpeza de registros
  órfãos.
- [ ] Implementar versão de esquema e migração de dados.
- [ ] Evitar dados sensíveis à ordem de carregamento ou IDs temporários.
- [ ] Definir herança de linhagem e transmissão cultural sem copiar
  automaticamente todas as habilidades.

**Entregáveis:** persistência versionada, migrações e testes de
restauração.  
**Aceite:** após salvar e reabrir, habilidades, origens e progressão
permanecem consistentes.

------------------------------------------------------------------------

## Fase 6 — IA autônoma de combate e aprendizado

**Versão proposta:** v0.7.0  
**Objetivo:** os NPCs usam seus conhecimentos sem depender do jogador.

### 6.1 IA de combate

- [ ] Selecionar alvo por distância, ameaça, prioridade e relação entre
  facções.
- [ ] Escolher entre ataque normal, técnica, magia, defesa, fuga e
  suporte.
- [ ] Respeitar mana, aura, recarga, alcance, linha de visão e risco
  para aliados.
- [ ] Distinguir perfis: mago de suporte, evocador, arqueiro, espadachim
  e comandante.
- [ ] Evitar spam de habilidades e ciclos de decisão excessivos.
- [ ] Adotar comportamento de grupo e recuo quando apropriado.

### 6.2 IA de aprendizado

- [ ] Buscar treino quando houver oportunidade e objetivo.
- [ ] Procurar professor, grimório ou instituição adequada.
- [ ] Escolher magia ou arma segundo aptidão, cultura, recursos e função
  social.
- [ ] Permitir decisões emergentes sem desbloqueios arbitrários.
- [ ] Adicionar intervalos e orçamento de processamento por NPC.

### 6.3 Observabilidade

- [ ] Exibir motivo da última escolha da IA em modo diagnóstico.
- [ ] Registrar decisões relevantes sem inundar o log.

**Entregáveis:** IA de combate e aprendizagem, perfis de NPC, limites de
processamento.  
**Aceite:** NPCs conjuram, treinam e evoluem autonomamente em simulações
repetidas.

------------------------------------------------------------------------

## Fase 7 — Instituições, professores e civilizações

**Versão proposta:** v0.8.0  
**Objetivo:** criar mecanismos sociais para difundir e preservar
conhecimento.

### Instituições

- [ ] Academias de magia e bibliotecas.
- [ ] Quartéis, campos de treinamento e escolas marciais.
- [ ] Guildas de aventureiros e ordens especializadas.
- [ ] Torres de magos, ordens arcanas e grupos ligados a patronos.
- [ ] Requisitos de recursos, população e tecnologia para criação e
  manutenção.

### Professores e discípulos

- [ ] Relação professor–discípulo com limite de alunos.
- [ ] Requisitos mínimos para ensinar uma magia ou técnica.
- [ ] Tempo de estudo, taxa de sucesso e especialização.
- [ ] Transferência de conhecimento entre cidades e reinos.
- [ ] Perda parcial de conhecimento quando mestres, bibliotecas ou
  instituições desaparecem.

### Política e cultura

- [ ] Preferências culturais por escolas, armas e tradições.
- [ ] Especialização militar e arcana de cada reino.
- [ ] Relações entre instituições, governantes e exércitos.
- [ ] Difusão por migração, conquista, alianças e comércio, quando
  suportado pela simulação.

**Entregáveis:** infraestrutura de ensino, transmissão de técnicas,
especialização de civilizações.  
**Aceite:** dois reinos desenvolvem estilos diferentes e conseguem
ensinar novas gerações.

------------------------------------------------------------------------

## Fase 8 — Magias avançadas, equipamentos e lendas

**Versão proposta:** v0.9.0  
**Objetivo:** completar o conteúdo avançado e ampliar as histórias
emergentes.

### 8.1 As 8 escolas

- [ ] **Abjuração:** escudos, resistência, proteção, antimagia.
- [ ] **Conjuração:** criaturas, objetos, reforços, portais.
- [ ] **Adivinhação:** detecção, previsão, revelação e suporte tático.
- [ ] **Encantamento:** coragem, medo, influência e controle com
  limites.
- [ ] **Evocação:** elementos, projéteis, explosões e grandes áreas.
- [ ] **Ilusão:** cópias, ocultação, distração e percepção alterada.
- [ ] **Necromancia:** drenagem, cadáveres, servos e energia vital.
- [ ] **Transmutação:** atributos, materiais, formas e terreno.

### 8.2 Entrega progressiva das 160 magias

- [ ] Círculos **0–1:** concluídos e testados nas fases iniciais.
- [ ] Círculos **2–3:** efeitos intermediários e combos.
- [ ] Círculos **4–5:** magias de campo e suporte em grupo.
- [ ] Círculos **6–7:** habilidades raras e alterações significativas.
- [ ] Círculos **8–9:** efeitos lendários, limites e contramedidas.
- [ ] Criar cenários de teste individuais por magia e escola.
- [ ] Verificar interações, resistências, duração, dano aliado e
  cancelamento.

### 8.3 Equipamentos e técnicas lendárias

- [ ] Armas e armaduras com propriedades especiais.
- [ ] Cajados, focos, grimórios e artefatos mágicos.
- [ ] Técnicas próprias desenvolvidas por grandes mestres.
- [ ] Nomes, títulos e feitos históricos de personagens lendários.
- [ ] Contramedidas para evitar supremacia absoluta de magos ou
  guerreiros.

**Entregáveis:** catálogo funcional completo, itens especiais,
personagens lendários.  
**Aceite:** as 160 magias têm efeito específico, regras, feedback visual
e testes de funcionamento.

------------------------------------------------------------------------

## Fase 9 — Balanceamento, desempenho e estabilidade

**Versão proposta:** v0.9.5  
**Objetivo:** preparar o mod para uso prolongado.

### Balanceamento

- [ ] Revisar mana, custo, cooldown, alcance, duração e dano.
- [ ] Testar progressão de personagens comuns, raros e lendários.
- [ ] Comparar magos, arqueiros, guerreiros, usuários de aura e
  exércitos mistos.
- [ ] Limitar invocações, controle mental, ressurreição e grandes
  efeitos em área.
- [ ] Criar parâmetros editáveis de dificuldade, raridade e velocidade
  de evolução.

### Desempenho

- [ ] Medir cenários de **20, 200 e 1.000 unidades**, conforme
  capacidade do PC.
- [ ] Limitar partículas, efeitos ativos, decisões de IA e consultas ao
  mapa.
- [ ] Utilizar cache, pooling, processamento em lotes e atualizações
  espaçadas.
- [ ] Detectar vazamento de memória, referências órfãs e erros
  repetitivos.

### Estabilidade

- [ ] Testar salvar/carregar, morte de unidades, mudança de reino e
  equipamento.
- [ ] Validar compatibilidade de versões e migrações.
- [ ] Revisar mensagens de erro, traduções PT-BR/EN e documentação.
- [ ] Verificar conflitos conhecidos com outros mods, quando possível.

**Entregáveis:** relatório de desempenho, configurações, correções e
conjunto de testes de regressão.  
**Aceite:** sessões prolongadas estáveis, sem perda de progressão e com
limites de desempenho documentados.

------------------------------------------------------------------------

## Fase 10 — Lançamento v1.0

**Objetivo:** publicar uma versão estável e documentada.

- [ ] Executar toda a bateria de testes e corrigir problemas
  bloqueadores.
- [ ] Produzir ZIP instalável separado do projeto de desenvolvimento.
- [ ] Preparar instruções para instalar, atualizar, remover e restaurar
  backup.
- [ ] Publicar changelog, compatibilidade, limitações conhecidas e
  créditos.
- [ ] Registrar versão exata de WorldBox/NML usada nos testes.
- [ ] Criar demonstração com batalha entre magos e guerreiros.
- [ ] Validar experiência de usuário: configurações, interface e
  feedback.
- [ ] Criar tag Git e pacote de release reproduzível.

**Definição de v1.0:** recursos anunciados funcionam na versão-alvo, são
testados, persistem quando necessário e possuem documentação de
instalação e limitações.

------------------------------------------------------------------------

# 4. Arquitetura técnica sugerida

``` text
ArcaneMartialAscension/
├── Code/
│   ├── Core/              # Estado de personagem, recursos e progressão
│   ├── GameIntegration/   # Adaptação para APIs do WorldBox e NML
│   ├── Spells/            # Catálogo, validação, runtime e efeitos
│   ├── Martial/           # Armas, posturas, combos, técnicas e aura
│   ├── Learning/          # Aptidões, estudo, treino, fontes de poder
│   ├── AI/                # Decisões de combate e aprendizado
│   ├── Institutions/      # Academias, mestres, guildas, cultura
│   ├── Persistence/       # Serialização, migração e restauração
│   ├── VisualEffects/     # Círculos, partículas, animações e pooling
│   ├── UI/                # Painéis, inspeção e configurações
│   └── Diagnostics/       # Logs, métricas e cenários de teste
├── Config/                # Balanceamento e catálogo descritivo
├── GameResources/         # Ícones, sprites, partículas
├── Locales/               # PT-BR e EN
└── mod.json
```

**Importante:** esta árvore é a **arquitetura-alvo**; não significa que
todas as pastas já existam na v0.4.0.

## 4.1 Modelos de dados propostos

### Personagem

- `actorId`: identificador estável.
- `classType`: mago, feiticeiro, bruxo, guerreiro ou outra
  especialização.
- `mana`, `maxMana`, `aura`, `maxAura`.
- `arcaneCircle`, `arcaneAffinityBySchool`.
- `knownSpellIds`, `spellMasteryById`, `cooldowns`.
- `weaponMasteryByType`, `knownTechniqueIds`.
- `traits`, `lineageId`, `patronId`, `teacherIds`, `institutionId`.
- `knowledgeOrigins`, `progressionVersion`.

### Origem de conhecimento

- `knowledgeId`: magia ou técnica.
- `sourceType`: estudo, mestre, combate, linhagem, pacto, evento,
  intervenção do jogador.
- `sourceId`: referência ao objeto ou personagem de origem, se existir.
- `learnedAt`: instante no mundo.
- `trainingProgress`, `masteryLevel`.

### Magia

- `id`, `name`, `school`, `circle`.
- `manaCost`, `cooldown`, `castTime`, `range`, `targeting`.
- `requirements`, `effectIds`, `vfxId`.
- `implementationStatus`: catalogada, implementada, testada.

**Regras de engenharia:** IDs estáveis; validação antes de custo; dados
serializáveis; compatibilidade entre versões; nenhuma DLL proprietária
distribuída.

------------------------------------------------------------------------

# 5. Dependências entre fases

``` text
Fase 0: Compilar e carregar
    ↓
Fase 1: Conjuração real e círculo visível
    ↓
Fase 2: Motor genérico + maestria funcional
    ↓
Fase 3: Animações de combate
    ↓
Fase 4: Aprendizado e origem de poderes
    ↓
Fase 5: Persistência
    ↓
Fase 6: IA autônoma
    ↓
Fase 7: Instituições e transmissão
    ↓
Fase 8: Magias avançadas e conteúdo lendário
    ↓
Fase 9: Balanceamento e otimização
    ↓
Fase 10: Release v1.0
```

**Observação:** algumas frentes podem ocorrer em paralelo, mas **IA de
aprendizado, instituições e conteúdo avançado dependem de um runtime e
de dados de progressão confiáveis**.

------------------------------------------------------------------------

# 6. Ordem de prioridades e entregas mínimas

| Prioridade      | Item                                  | Primeira versão-alvo | Por que importa                         |
|-----------------|---------------------------------------|----------------------|-----------------------------------------|
| P0              | Compilação e carregamento             | 0.4.1                | Sem isso não existe teste no jogo       |
| P0              | Círculo visível + magia com dano real | 0.4.1                | Resolve o problema atual                |
| P0              | Mana e recarga sem cobrança indevida  | 0.4.1                | Garante regras corretas                 |
| P1              | Motor de efeitos reutilizável         | 0.4.2                | Permite implementar as 160 magias       |
| P1              | Maestria por arma real                | 0.4.2                | Consolida a evolução dos guerreiros     |
| P1              | Animações de combate                  | 0.5.0                | Melhora leitura e impacto das batalhas  |
| P1              | Aprendizado com origem                | 0.6.0                | Evita poderes surgindo sem explicação   |
| P1              | Persistência                          | 0.6.1                | Protege o progresso dos habitantes      |
| P2              | IA autônoma                           | 0.7.0                | Faz o mundo evoluir sozinho             |
| P2              | Academias, mestres e guildas          | 0.8.0                | Cria transmissão social de conhecimento |
| P2              | 160 magias funcionais                 | 0.9.0                | Completa o grimório                     |
| P0 para release | Estabilidade e performance            | 0.9.5–1.0            | Torna o mod utilizável                  |

------------------------------------------------------------------------

# 7. Plano de testes obrigatório

## 7.1 Checklist por funcionalidade

- [ ] **Build:** compila sem erros no NML da versão-alvo.
- [ ] **Carregamento:** mod aparece e registra seus recursos.
- [ ] **Renderização:** F5 mostra círculo; círculo de conjuração aparece
  no personagem/alvo correto.
- [ ] **Conjuração:** efeito real é aplicado e o alvo sofre a
  consequência esperada.
- [ ] **Mana:** só diminui quando a ação válida é executada.
- [ ] **Magias pendentes:** não causam efeitos fictícios nem gastam
  recursos.
- [ ] **Maestria:** XP e níveis evoluem com a arma correta.
- [ ] **Aprendizado:** magia avançada exige fonte e requisitos.
- [ ] **Histórico:** origem da habilidade pode ser consultada.
- [ ] **IA:** NPC usa habilidades e aprende sem intervenção manual.
- [ ] **Persistência:** salvar/reabrir mantém os dados.
- [ ] **Performance:** comparar 20, 200 e 1.000 unidades.
- [ ] **Regressão:** funcionalidades antigas continuam funcionando após
  mudanças.

## 7.2 Evidência mínima para marcar uma tarefa como concluída

1.  Arquivo(s) de código alterado(s) e resumo da implementação.
2.  Resultado de compilação ou validação estática correspondente.
3.  Passos reproduzíveis de teste dentro do WorldBox.
4.  Resultado observado e, se houver falha, trecho do `Player.log`.
5.  Registro de limitações, riscos e próximos passos.

**Regra:** *“Código escrito” ≠ “funcionalidade concluída”*.

------------------------------------------------------------------------

# 8. Backlog de melhorias pós-v1.0 (opcional)

- [ ] Novos estilos de luta e armas adicionais.
- [ ] Especializações e subclasses.
- [ ] Eventos de despertar, torneios e guerras entre ordens.
- [ ] Artefatos únicos e relíquias históricas.
- [ ] Sistema de reputação, títulos e feitos lendários.
- [ ] Rivalidades entre escolas e tradições.
- [ ] Editor de magias e técnicas configurável.
- [ ] Compatibilidade ampliada com novas versões do WorldBox.
- [ ] Ferramentas para criadores de conteúdo e outros mods.

Essas ideias **não bloqueiam** o lançamento v1.0.

------------------------------------------------------------------------

# 9. Instruções para Codex no VS Code

Ao executar uma fase, seguir este procedimento:

1.  Ler `AGENTS.md`, `README.md`, `docs/ARQUITETURA.md` e este roadmap.
2.  Inspecionar código existente antes de criar arquivos ou sistemas
    duplicados.
3.  Confirmar APIs reais nas DLLs locais do WorldBox 0.51.2.
4.  Escolher **uma tarefa pequena** e definir o teste que provará seu
    funcionamento.
5.  Implementar com separação entre regras de jogo, integração e visual.
6.  Rodar scripts de validação, compilar e executar teste no jogo, se o
    ambiente permitir.
7.  Registrar logs, problemas conhecidos e alterações.
8.  Atualizar os checkboxes e o changelog **somente após evidência de
    teste**.

## Prompt inicial recomendado

> Leia `AGENTS.md` e `ArcaneMartialAscension_ROADMAP_COMPLETO.md`.
> Estamos na v0.4.0 Alpha e a primeira prioridade é a Fase 0 seguida da
> Fase 1. Inspecione as APIs reais do WorldBox 0.51.2 e NeoModLoader
> 1.2.1.1 instalados neste computador. Corrija erros de compilação e
> faça F5 mostrar um círculo, depois implemente uma magia com dano real
> sem consumir mana em caso de falha. Não avance para novos sistemas até
> validar esses critérios. Informe arquivos alterados, testes
> executados, resultados e limitações. Não afirme que o jogo foi testado
> se não foi aberto.

------------------------------------------------------------------------

# 10. Resumo executivo das versões

| Versão          | Tema central   | Marco de conclusão                                  |
|-----------------|----------------|-----------------------------------------------------|
| **0.4.0 Alpha** | Base existente | Catálogo e protótipos, ainda sem validação completa |
| **0.4.1**       | Fundação       | Compilação, círculo e primeira magia real           |
| **0.4.2**       | Mecânicas      | Executor genérico e maestria confiável              |
| **0.5.0**       | Animações      | Combate visual de magos e guerreiros                |
| **0.6.0**       | Aprendizado    | Origem, treino e evolução natural                   |
| **0.6.1**       | Persistência   | Progresso mantido entre sessões                     |
| **0.7.0**       | IA             | NPCs combatem e aprendem sozinhos                   |
| **0.8.0**       | Civilizações   | Academias, guildas, mestres e discípulos            |
| **0.9.0**       | Conteúdo       | Magias avançadas, itens e técnicas lendárias        |
| **0.9.5**       | Qualidade      | Balanceamento, desempenho e estabilidade            |
| **1.0.0**       | Lançamento     | Mod testado, documentado e distribuível             |

------------------------------------------------------------------------

**Próxima ação recomendada:** abrir a v0.4.0 no VS Code e começar pela
**Fase 0**. O primeiro objetivo concreto é fazer o mod compilar e exibir
corretamente o círculo mágico antes de ampliar as animações e os
sistemas de aprendizado.
