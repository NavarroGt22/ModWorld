# Formação natural e demonstração visual — 28/10/2026

Expansão experimental das Fases 2/3/4/6, autorizada pelo usuário. Não é
conclusão dessas fases e não altera o manifesto v0.4.0 Alpha.

## Formação e aprendizado

O observador considera habitantes vivos, sapientes, adultos, acordados e em
paz. A mesma função estável de aptidão por ID foi preservada; suas faixas
agora selecionam 7% Mago, 2% Bruxo e 3% Feiticeiro. Isso não garante essas
proporções em uma população pequena. Tradições existentes são respeitadas.

- Mago inicia estudo próprio, sem receber uma magia ao criar o estado.
- Feiticeiro precisa de 24 unidades de tempo observado para despertar uma
  origem estável: Dracônica, Elemental, Feérica ou Selvagem.
- Bruxo precisa de 64 unidades observadas para concluir um ritual autônomo
  e firmar pacto com Brasa Ancestral ou Guardião do Véu. Esses são patronos
  narrativos do protótipo, não Actors invocados ou NPCs com agenda.
- A formação é concedida uma vez por indivíduo durante a sessão. Enquanto
  ritual/despertar ocupa o tempo em paz, não se concede prática simultânea.
- Nenhum pacto, origem, XP ou feitiço é concedido só por abrir a ficha.

Depois da formação, cada caminho pratica quatro magias dos círculos 0–1.
Bruxos usam Agulha Etérea, Toque Frio, Dreno de Vigor e Lâmina Invocada.
Feiticeiros Feéricos usam Conjuração/Transmutação; as outras origens e os
Magos usam o currículo inicial de Evocação. A herança de linhagem, poderes
exclusivos por patrono e o aprendizado acima do círculo 1 continuam pendentes.

Cada sessão de aprendizado exige trabalho real observado. Trocar tradição
não transfere trabalho parcial. Pausa, sono, combate e desativação do
observador não geram estudo retroativo; saltos por observação são limitados.
As magias registram sua tradição de aprendizado. Círculo e XP de conjuração
usam essa tradição, sem converter Bruxos/Feiticeiros em Magos. A ficha exibe
patrono/origem e método de aprendizado. Um traço sozinho não cria repertório.

## Integração nativa e correção de nascimento

Foi removida a escrita silenciosa por reflection no campo legado `birth`.
Os campos `ActorTrait.rate_birth`, `rate_acquire_grow_up` e `rate_inherit`
foram verificados na DLL original instalada: inteiros. O registro agora
desativa nascimento, crescimento, herança e sorteio nativo dos traços de
progressão; formação e equipamento controlam sua atribuição explicitamente.
Não foi inventada uma conversão de porcentagem para os pesos nativos.

Evidência estática REA: `ev_b06a273eed35f297f02e5dc69d44ab05e6f6a90b5155932e9161a93e7da13ae8`,
página de campos `ev_8858a11928b1260cc7765ef4a96b41e54e1d52a1a78557de4c8e55196a5200d2`.
Assembly original SHA-256 `51d275f0168be2f6ca26341ab292406714e694e0270eafcb25b999d5df6dd69f`,
MVID `3aa7d3da-1573-446f-8db1-3cd80acc5c24`. A inspeção é estática, não execução.

## Combate e visuais

O seletor de magia é compartilhado pela integração e pelos testes de regras.
Usa magias conhecidas e ofensivas, respeitando alvo inimigo, escola, círculo,
mana, alcance, recarga e reserva por conjurador. Se a magia preferida não
puder ser usada, procura outra válida. Não escolhe buffs de suporte nesta
primeira IA. Um efeito concreto continua sendo requisito para cobrar o custo;
magias de dano precisam confirmar a perda de vida.

A primeira demonstração visual usa composição própria de efeitos:

- Corte com direção do atacante para o alvo e três quadros de arco no sprite.
- Projétil com rastro e estrela de impacto, nas cores da escola.
- Lança Elétrica instantânea com feixe segmentado e três variações de desenho.
- Visual de apoio via interface para corte/raio, além de círculos/projéteis.
- Tempo do mundo para voo, efeitos e intervalo entre decisões de conjuração;
  pausa congela o avanço. Trocar de mundo limpa efeitos e reservas.
- Reuso de até 48 objetos de sprite; limite separado de 32 efeitos de apoio
  e 128 voos. Saturação visual não concede dano ou custo fictícios.

A demonstração ainda não anima o corpo do Actor e não implementa bloqueio,
esquiva ou combos. Não foram copiados sprites/código de Combat Gods. A
qualidade estética em vários zooms e o desempenho com muitas unidades
continuam exigindo refinamento e medição.

## Verificações e evidência no jogo

Suite de 74 cenários: 58 anteriores e 16 novos. Os novos cobrem formação,
origem/pacto, limites de tempo, interrupções, propriedade por indivíduo,
currículos, XP por tradição e ciclo completo de aprender/selecionar/conjurar
com executor simulado. Esses testes não substituem WorldBox.

No WorldBox instalado foi gerado um mundo descartável, sem carregar ou
salvar por cima de um mundo do usuário. Quarenta humanos foram adicionados;
nenhum recebeu uma magia por botão. A população cresceu normalmente.
Registros observados:

- Actor 34: `tradition=Sorcerer origin=Elemental`, seguido de aprendizado
  de Centelha e Estilhaço por `source=Awakening`.
- Actors 30 e 52: pactos `ama.patron.veil` e `ama.patron.ember`, seguidos
  de Agulha Etérea e Toque Frio por `source=Pact`.
- Actors 43 e 53: formação de Mago e aprendizado por `source=SelfStudy`.

Três ursos foram adicionados perto da vila para provocar combate real.
Sem F8 ou ensino manual, o log registrou:

```text
[AMA AI] casting actor=52 tradition=Warlock spell=ama.spell.conjuration.0.agulha_eterea target=100
[AMA Cast] launched spell=ama.spell.conjuration.0.agulha_eterea caster=52 target=100 mana=121,2 manual=False
[AMA Cast] applied spell=ama.spell.conjuration.0.agulha_eterea caster=52 target=100 hp=214->209 damage=5 mana=119,8 cooldown=2
[AMA Cast] result spell=ama.spell.conjuration.0.agulha_eterea: Agulha Etérea: efeito aplicado. Vida -5 · Mana -3
[AMA Cast] applied spell=ama.spell.evocation.0.centelha_rubra caster=43 target=100 hp=199->192 damage=7 mana=97 cooldown=2
[AMA AI] casting actor=34 tradition=Sorcerer spell=ama.spell.evocation.1.lanca_eletrica target=100
[AMA Cast] applied spell=ama.spell.evocation.1.lanca_eletrica caster=34 target=100 damage=12 mana=125 manual=False
```

Também houve cancelamento de projéteis por morte do alvo sem cobrar mana.
Regeneração durante o voo explica diferenças entre a mana do lançamento e
a mana após o impacto; o resultado registra separadamente o custo de 3.
Círculos de conjuração foram vistos durante essa luta. Isso confirma casos
de combate automático dos três caminhos, não todos os perfis ou teste de carga.

Foi aberta a ficha de Sihed, sem formação arcana, sem conceder conhecimento
por inspeção. O texto de formação coube sem sobrepor a seção seguinte.
Após a luta, F9 foi usado para demonstração manual de corte; não é evidência
de técnica de espada aplicada em um ataque natural. A revisão posterior
sincronizou também o relógio do desenho de apoio com a pausa.

A revisão final compilou 24 fontes sem avisos/erros e passou novamente nos
74 cenários, na validação do catálogo/traduções e em `git diff --check`.
Foi instalada e aberta em outro mundo descartável. Em Thegik, o corte manual
ficou visível e congelado durante a pausa, inclusive após abrir o F6; ao
retomar, expirou. A Lança Elétrica foi ensinada manualmente só para esse teste
visual: pausada foi rejeitada com mana preservada; após retomar, aplicou 13
de dano e cobrou 10 de mana (`caster=34 target=36 manual=True`). Não se deve
confundir esse preparo manual com a evidência autônoma do primeiro mundo.
O círculo foi visto; o curto feixe ainda precisa de revisão estética em zoom
próximo, pois o quadro capturado não permite avaliar sua sequência completa.
Não houve exceção de integração ou de desenho nesses logs.

## Backups e limites

- Saves/autosaves: `C:\Users\rn558\Documents\AMA_TestBackups\NaturalMagic_20261028_183005`.
- Mod anterior: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_183010`.
- Backup antes da revisão final dos visuais: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_184413`.
- Log de formação/combate: `Player_natural_formation_combat.log` dentro do
  backup de saves. Preserva a primeira revisão executada no jogo.
- Log da revisão final: `Player_final_visual.log` no mesmo backup. Os 24
  fontes e duas traduções instalados foram conferidos por hash contra o
  workspace; não há DLL no mod instalado. O jogo foi fechado após os testes.

Ainda não há persistência de XP, origens, pactos ou magias ao carregar save.
Perfis próprios de dragões/demônios, obrigações de patrono, herança entre
gerações, quatro armas em combate e aceites completos das Fases 2/3/4/6
permanecem pendentes. As diferenças entre observação real e regras simuladas
devem ser preservadas em qualquer comunicação sobre esta entrega.
