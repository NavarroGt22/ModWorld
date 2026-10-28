# Pactos, linhagens e persistência — 28/10/2026

Pedido do usuário: ampliar pactos e origens com referências de D&D e
prosseguir no roadmap. Esta entrega aprofunda a Fase 4 e implementa a base
de persistência da Fase 5. Pendências de animação da Fase 3 permanecem abertas.

## Referências e adaptação

Arquétipos consultados nas fontes oficiais:

- [Warlock — revisão de 2024](https://www.dndbeyond.com/posts/1756-2024-warlock-vs-2014-warlock-whats-new): Corruptor, Arquifada, Celestial e Grande Antigo.
- [Sorcerer — revisão de 2024](https://www.dndbeyond.com/posts/1769-2024-sorcerer-vs-2014-sorcerer-whats-new): origens Dracônica, Selvagem, Aberrante e Mecânica.

Os nomes e temas orientam os perfis. Atributos, valores de mana, currículos,
ícones existentes do AMA e regras de progressão são uma adaptação para
WorldBox, não uma reprodução das subclasses completas do jogo de mesa.
Não foram copiados textos de regras, ilustrações ou tabelas de poderes.

## Perfis funcionais

`Core/ArcaneIdentities.cs` centraliza IDs, nomes, atributos, reserva de mana
e quatro magias iniciais por perfil. O mesmo catálogo alimenta o registro
nativo de traços, o aprendizado, a capacidade de mana e a ficha do personagem.

- Corruptor: +15 vida, +4 dano, +10 mana; fogo e energia vital.
- Arquifada: +2 armadura, +4 cadência, +15 mana; manifestação e transformação.
- Celestial: +30 vida, +2 armadura, +15 mana; proteção e manifestação.
- Grande Antigo: +2 dano, +2 cadência, +30 mana; manifestação e energia vital.
- Dracônica: +25 vida, +3 armadura, +10 mana; poder elemental e proteção.
- Selvagem: +4 dano, +25 mana; currículo elemental.
- Aberrante: +10 vida, +3 cadência, +30 mana; manifestação e energia vital.
- Mecânica: +4 armadura, +2 cadência, +15 mana; proteção e transformação.

Os bônus são adicionais aos da tradição e só o perfil correspondente fica
ativo. Atualizar a ficha não acumula bônus nem preenche mana gratuitamente.
Cada repertório tem quatro entradas dos círculos 0–1 com executor existente;
conhecê-las continua exigindo tempo e círculo suficientes. Magias de suporte
ainda não são escolhidas pela IA ofensiva.

Foram registrados 10 traços com traduções BR/EN no grupo Pactos e linhagens:
os oito perfis principais e as origens antigas Elemental/Feérica, preservadas
para compatibilidade. Novos feiticeiros escolhem entre as quatro origens
principais. A seleção por ID continua estável, dentro das vocações já existentes.

Migrações: `ama.patron.ember` passa a Corruptor; `ama.patron.veil`, a Grande
Antigo. IDs de patronos personalizados desconhecidos são preservados sem
inventar atributos. Os valores existentes de `SorcerousOrigin` não foram
reordenados; Aberrante/Mecânica foram acrescentadas ao final.

A ficha passa a ter a linha Pacto / linhagem e uma área com os atributos
da identidade. Sopro dracônico, surtos selvagens, telepatia, teleporte feérico,
cura celestial e obrigações de patrono continuam explicitamente pendentes.
Linhagem aqui identifica a origem mágica; herança biológica entre gerações
ainda não foi implementada.

## Persistência no save nativo

`ActorProgressionPersistence` intercepta `Actor.prepareForSave()` antes de
o jogo preparar seus dados. A chave `ama.progression.v1` é gravada no campo
personalizado de string do próprio Actor por `BaseSystemData.set/get`.
O payload acompanha o save do mundo, sem um arquivo global por ID de unidade.

Não se acessa diretamente o campo interno `Actor.data`: a integração usa
o método público `Actor.getData()`, verificado na DLL instalada. A preparação
do snapshot ocorre antes de `Actor.finishSaving()`, chamado pelo método nativo.

Dados incluídos:

- XP das três tradições, oito escolas e quatro armas; círculos são derivados.
- Magias conhecidas, tradição de aprendizado, recargas restantes e magia preparada.
- Técnicas conhecidas, usos confirmados, nível derivado e recarga restante.
- Mana, aura e despertar da aura; capacidades são recalculadas pelo catálogo.
- Patrono, origem, formação concluída ou em andamento.
- Trabalho parcial de estudo/prática e origem de cada conhecimento.
- Bônus temporários pertencentes ao executor, com duração restante.

O carregamento é gradual quando o personagem é observado ou inspecionado.
Restaurar dados existentes não concede estudo retroativo. Conjurações em voo
não são restauradas; o projétil ainda não aplicado não havia cobrado mana.
Snapshots de outro Actor são ignorados, sem copiar poderes para clones.

Dados inválidos ou de uma versão não suportada são mantidos intactos e
bloqueiam nova progressão desse Actor, com diagnóstico no log. A validação
é atômica: só publica o personagem restaurado depois de conferir o conteúdo.
Há limites de tamanho, profundidade, quantidade de registros, valores de XP,
recargas, buffs permitidos e requisitos de aprendizado. Não há desserialização
polimórfica ou restauração de objetos Unity a partir do JSON.

O estado em memória também confere ID e referência do ActorData para evitar
reaproveitar o progresso quando o jogo recicla um objeto Actor. Apenas os
patches Harmony deste módulo são removidos ao descarregar o mod.

## Evidência estática

DLL original: SHA-256 `51d275f0168be2f6ca26341ab292406714e694e0270eafcb25b999d5df6dd69f`,
MVID `3aa7d3da-1573-446f-8db1-3cd80acc5c24`.

- REA `ev_6000d46325024f9a6660efdb97989db35bb9d5d47d2f33964043297e8ff6a060`:
  metadados de Actor/ActorData/MapStats.
- REA `ev_a04c928e612f4719f6076100a1c5f1598be0939de4f15786acba0d751240c7ca`:
  BaseSystemData, BaseObjectData e SaveCustomData, incluindo get/set de string.
- Mono.Cecil confirmou `Actor.getData()` público/virtual retornando o ActorData
  interno e a chamada final de `finishSaving()` em `prepareForSave()`.

Essas são evidências de API/estrutura, não confirmação de restauração no jogo.

## Validação

99 cenários de regras aprovados, 27 fontes compiladas sem avisos/erros.
Os 25 cenários novos cobrem repertórios de todos os perfis, seleção estável,
migração, bônus idempotentes e round trip de progresso; também cobrem dados
malformados, limites, Actor incorreto, esquema futuro, fontes de poder,
recargas, técnicas, origens e ausência de progresso durante tempo offline.

`scripts/validate.ps1` aprovou 160 magias, 16 ícones e 59 traços com traduções.
No jogo, os 59 traços foram registrados com atributos nativos verificados.
Em uma população de teste foram observados Arquifada (Actor 30), Grande
Antigo (52), Dracônica (34) e Selvagem (60), com os repertórios correspondentes.
Foi observado treino de lança em paz alcançando nível 1; isso não valida
acertos naturais das quatro armas.

Na preparação do save, o hook executou sem erros e registrou snapshots dos
quatro personagens com quatro magias cada. Mana: Arquifada 150, Grande
Antigo 165, Dracônica 145 e Selvagem 160. Maleora (Actor 21) recebeu apenas
para controle manual 30 XP de espada, Corte preciso I e aura 45/50.

O slot 4 foi conferido vazio tanto na interface quanto no filesystem.
Após autorização explícita do usuário, o mundo `AMA - QA Pactos e Persistencia`
foi gravado nesse slot. Os cinco arquivos nativos do save foram conferidos,
o WorldBox foi encerrado por completo e iniciado novamente; o mundo foi
reaberto pela interface de mundos salvos.

Os 28 Actors com snapshot registrado antes de encerrar tiveram restauração
registrada na nova sessão. A comparação por Actor de quantidade de magias,
XP de espada, mana, patrono e origem encontrou **zero diferenças**. Entre eles:

- Actor 30: Arquifada, quatro magias e 150 mana.
- Actor 52: Grande Antigo, quatro magias e 165 mana.
- Actor 34: Dracônica, quatro magias e 145 mana.
- Actor 60: Selvagem, quatro magias e 160 mana.
- Actor 21, Maleora: 30 XP de espada e 100 mana. A ficha reaberta confirmou
  grau Aprendiz 1/7, 30/100 XP e Corte preciso I.

O diagnóstico de Maleora confirmou aura despertada, com capacidade 50.
A simulação rodou após o carregamento antes da pausa de inspeção; o valor
observado foi 41/50. Portanto este cenário não comprova igualdade imediata
dos 45/50 vistos antes da gravação, nem recargas/buffs em andamento. Esses
campos têm cobertura nos testes de regras; falta um aceite nativo isolado.
O pulso periódico de aura ainda usa `Time.time`, e seu comportamento durante
pausa/carregamento precisa de revisão separada.

Não houve exceções, falhas do aprendizado ou snapshots inválidos/incompatíveis
no log da sessão restaurada. O jogo foi encerrado após a inspeção. Este aceite
confirma a persistência básica no cenário descrito, sem concluir toda a Fase 5.

Evidências no diretório de backup abaixo: `Player_before_restart.log`,
`Player_after_restart.log`, `expected_actors.json`, `persistence_comparison.json`
e `native_acceptance.json`, além da cópia dos arquivos gravados em
`QA_save4_before_restart/`.

Backup de saves/autosaves: `C:\Users\rn558\Documents\AMA_TestBackups\PactsPersistence_20261028_192540`.
Backup do mod anterior: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261028_192545`.
Os 15 arquivos dos saves 1, 2 e 3 mantiveram hashes idênticos aos backups.

## Pendências da fase

Herança entre gerações, conhecimento de facções/instituições (ainda não
implementadas), poderes completos de cada arquétipo, validação de carga e
aceites completos das quatro armas continuam pendentes. Esta entrega não
declara todas as Fases 3, 4 e 5 concluídas.
