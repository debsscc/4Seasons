// using UnityEngine;

// public class EventController : MonoBehaviour {

//     //public List<SeatButton> seatButtons; 

//     public void CarregarMapa(MapaCinema.MapaCinema mapa)
//     {
//         // Primeiro, trava tudo
//         //foreach (var b in seatButtons)
//             b.isFree = false;

//         //if (mapa == EventController.MapaCinema.MapaA)
//         {
//             LiberarAssento(ItemOpcao.H3);
//             LiberarAssento(ItemOpcao.H4);
//             LiberarAssento(ItemOpcao.H5);
//             LiberarAssento(ItemOpcao.H6);
//             LiberarAssento(ItemOpcao.H7);
//         }
//         else
//         {
//             LiberarAssento(ItemOpcao.H1);
//             LiberarAssento(ItemOpcao.H2);
//             LiberarAssento(ItemOpcao.H5);
//         }

//         AtualizarVisual();
//     }

//     void LiberarAssento(ItemOpcao opcao)
//     {
//         var botao = seatButtons.First(s => s.assento == opcao);
//         botao.isFree = true;
//     }

//     public void AtualizarVisual()
//     {
//         foreach (var b in seatButtons)
//             b.AtualizarVisual();
//     }
// }
