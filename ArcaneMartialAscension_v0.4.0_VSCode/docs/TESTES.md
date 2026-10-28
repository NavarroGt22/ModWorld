# Checklist de validação — v0.4.0

Use um mundo novo/descartável e faça backup dos saves importantes.

| Etapa | Ação | Resultado esperado | Situação atual |
|---|---|---|---|
| 1 | `AMA: 1 - Validar catálogo e estrutura` | 160 magias/8 escolas/IDs únicos | Passou em 27/10/2026 |
| 2 | `AMA: 2 - Compilar C# (DLLs locais)` | Nenhum erro C# | Passou com Roslyn NML; 0 erros/avisos |
| 3 | `AMA: 3 - Instalar fontes no WorldBox` | `Mods/ArcaneMartialAscension/mod.json` | Instalado com backup em 27/10/2026 |
| 4 | Abrir WorldBox com Modo Experimental | Mod v0.4.0 no NML, sem erros | Carregou; painel Tracos OK; ver relatório |
| 5 | F5 sobre o mapa | Círculo laranja ~1.5s | Círculo GUI observado em 27/10/2026 |
| 6 | Criar 2 unidades, zoom, F7 na primeira | Nome aparece no painel | Não verificado |
| 7 | Evocação > Centelha Rubra > Aprender > F8 sobre segunda | Dano + círculo + redução de mana só no sucesso | Não verificado |
| 8 | Selecionar magia avançada e F8 | 'MECANICA PENDENTE', sem custo de mana | Não verificado |
| 9 | F9 (Espada, Lança, Arco, Machado) | XP e nível individual por arma | Não verificado |
| 10 | F10 | Traço de aura e efeito visual | Não verificado |
| 11 | Ataques de NPC com traços | XP automático de maestria por combate | Não verificado |
| 12 | Salvar e reiniciar | Anotar o que persiste e o que não | Sistema atual em memória |

Se houver erros, rode a tarefa **AMA: 4 - Ver erros do Player.log**, anote etapa do teste e envie
`Player.log` e `Player-prev.log`. Separar falhas de compilação de falhas de jogo/visual.

Evidências, ambiente, backups e limites desta revisão: `VALIDACAO_FASE_0.md`.
O F5 comprova o overlay GUI; não comprova posição de conjuração, projétil ou dano.
