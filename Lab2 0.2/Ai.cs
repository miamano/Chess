﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2_0._2
{
    public class Ai
    {
        Ui _ui = null;
        public int _roundsWithoutPawnMove = 0;

        public Ai(Ui ui)
        {
            _ui = ui;
        }
        public void CalculateBestMove(Player currentPlayer, Player opponentPlayer)
        {
            // key är en array på två där index 0 är X och index 1 är Y, Value är den pice som kan flytta dit.
            Dictionary<int[], Piece> safeMoves = new Dictionary<int[], Piece>();
            Dictionary<int[], Piece> unSafeMoves = new Dictionary<int[], Piece>();

            // Key är spelarens pjäs och value är den pjäs som spelaren kan klå med key pjäsen.
            Dictionary<Piece, Piece> safeBeatMoves = new Dictionary<Piece, Piece>();
            Dictionary<Piece, Piece> unSafeBeatMoves = new Dictionary<Piece, Piece>();

            //fyller listorna med moves.
            AddMovesToLists(safeMoves, unSafeMoves, safeBeatMoves, unSafeBeatMoves, currentPlayer, opponentPlayer);

            //Försöker först hota motståndarens kung om det inte går fortsätter metoden in i if satsen.
            if (!MoveToThreatOpponentKing(currentPlayer, opponentPlayer))
            {
                if (safeBeatMoves.Count() != 0)
                {
                    Piece ownPiece = null;
                    Piece opponentPiece = null;
                    foreach (KeyValuePair<Piece, Piece> move in safeBeatMoves)
                    {
                        if (opponentPiece == null)
                        {
                            ownPiece = move.Key;
                            opponentPiece = move.Value;
                        }
                        else
                        {
                            if (opponentPiece.Points < move.Value.Points)
                            {
                                ownPiece = move.Key;
                                opponentPiece = move.Value;
                            }
                        }
                    }

                    BeatIfColide(opponentPiece.PosX, opponentPiece.PosY, ownPiece, opponentPlayer);
                    return;
                } 
                if (unSafeBeatMoves.Count() != 0)
                {

                    Dictionary<Piece, Piece> rankedMoves = new Dictionary<Piece, Piece>();

                    //Går igenom osäkra moves för att se om pjäsen spelaren offrar är värd mer än den spelaren tar.
                    foreach (KeyValuePair<Piece, Piece> move in unSafeBeatMoves)
                    {
                        // om egen pjäs är värd mer än motståndarens.
                        if (move.Key.Points < move.Value.Points) //TODO, anpassa för mer action :D
                        {
                            // om egen pjäs redan är rankad
                            if (rankedMoves.ContainsKey(move.Key))
                            {
                                // Om rankad pjäs mål är värt mindre än det nya målet.
                                if (rankedMoves[move.Key].Points < move.Value.Points)
                                {
                                    rankedMoves[move.Key] = move.Value;
                                }
                            }
                            else
                            {
                                rankedMoves.Add(move.Key, move.Value);
                            }
                        }
                    }

                    if (rankedMoves.Count() != 0)
                    {
                        Piece ownPiece = null;
                        Piece opponentPiece = null;

                        foreach (KeyValuePair<Piece, Piece> move in rankedMoves)
                        {
                            if (opponentPiece == null)
                            {
                                ownPiece = move.Key;
                                opponentPiece = move.Value;
                            }
                            else
                            {
                                if (opponentPiece.Points < move.Value.Points && ownPiece.Points > move.Key.Points)
                                {
                                    ownPiece = move.Key;
                                    opponentPiece = move.Value;
                                }
                            }
                        }

                        BeatIfColide(opponentPiece.PosX, opponentPiece.PosY, ownPiece, opponentPlayer);
                        return;
                    }
                }
                if (safeMoves.Count() != 0)
                {
     
                        while (true)
                    {                       
                        Random random = new Random();
                        int posX = random.Next(8);
                        int posY = random.Next(8);

                        foreach (KeyValuePair<int[], Piece> move in safeMoves)
                        {
                            if (posX == move.Key[0] && posY == move.Key[1])
                            {
                                BeatIfColide(posX, posY, move.Value, opponentPlayer);
                                return;
                            }
                        }

                        
                    }
                }
                if (unSafeMoves.Count() != 0)
                {

                    Piece ownPiece = null;
                    int posX = -1;
                    int posY = -1;

                    foreach (KeyValuePair<int[], Piece> move in unSafeMoves)
                    {
                        if (ownPiece == null)
                        {
                            ownPiece = move.Value;
                            posX = move.Key[0];
                            posY = move.Key[1];
                        }
                        else
                        {
                            if (ownPiece.Points > move.Value.Points)
                            {
                                ownPiece = move.Value;
                                posX = move.Key[0];
                                posY = move.Key[1];
                            }
                        }
                    }

                    BeatIfColide(posX, posY, ownPiece, opponentPlayer);
                    return;



                }

                _ui.LoggCheckMate(currentPlayer);
            }
        }

        private void AddMovesToLists(Dictionary<int[], Piece> safeMoves, Dictionary<int[], Piece> unSafeMoves, Dictionary<Piece, Piece> safeBeatMoves, Dictionary<Piece, Piece> unSafeBeatMoves, Player currentPlayer, Player opponentPlayer)
        { 
            // Lägger in säkra moves och osäkra moves i vars en dict 
            foreach (Piece ownPiece in currentPlayer.Pieces)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        bool tmpIsMoveValid = ownPiece.IsMoveValid(x, y, currentPlayer, opponentPlayer);
                        bool tmpIsKingSafeAfterMove = IsKingSafeAfterMove(x, y, ownPiece, currentPlayer, opponentPlayer);

                        //Safe moves
                        if (tmpIsMoveValid && tmpIsKingSafeAfterMove && !IsSquareThreaten(x, y, currentPlayer, opponentPlayer))
                        {
                            safeMoves.Add(new int[] { x, y }, ownPiece);
                        }

                        //Unsafe moves
                        else if (tmpIsMoveValid && tmpIsKingSafeAfterMove)
                        {
                            unSafeMoves.Add(new int[] { x, y }, ownPiece);
                        }

                        /* 
                        //OBS! Dyra operationer ska brytas ut    
                        //Safe moves
                        if (ownPiece.IsMoveValid(x, y, currentPlayer, opponentPlayer) && IsKingSafeAfterMove(x, y, ownPiece, currentPlayer, opponentPlayer) && !IsSquareThreaten(x, y, currentPlayer,opponentPlayer))
                        {
                            safeMoves.Add(new int[] { x, y }, ownPiece);
                        }

                        //Unsafe moves
                        else if (ownPiece.IsMoveValid(x, y, currentPlayer, opponentPlayer) && IsKingSafeAfterMove(x, y, ownPiece, currentPlayer, opponentPlayer))
                        {
                            unSafeMoves.Add(new int[] { x, y }, ownPiece);
                        }*/
                    }
                }
            }

            //Går igenom motståndarens pjäser för att se om dessa går att nå genom safe moves och unsafe moves.
           foreach(Piece opponentPiece in opponentPlayer.Pieces)
           {
                foreach (KeyValuePair<int[], Piece> SafeMove in safeMoves)
                {
                    // Om ett move matchar en av motsåndarens positioner
                    if (opponentPiece.PosX == SafeMove.Key[0] && opponentPiece.PosY == SafeMove.Key[1])
                    {
                        // Kontrollerar om egen pjäs redan har ett lagrat move
                        if (safeBeatMoves.ContainsKey(SafeMove.Value))
                        {
                            // Om pjäsen redan har ett move kontrollera om det nya movet har högre värde än de gammla.
                            if (safeBeatMoves[SafeMove.Value].Points < opponentPiece.Points)
                            {
                                safeBeatMoves[SafeMove.Value] = opponentPiece;
                            }
                        }
                        else
                        {
                            safeBeatMoves.Add(SafeMove.Value, opponentPiece);
                        }
                    }
                }
                foreach (KeyValuePair<int[], Piece> unSafeMove in unSafeMoves)
                {
                    // Om ett move matchar en av motsåndarens positioner
                    if (opponentPiece.PosX == unSafeMove.Key[0] && opponentPiece.PosY == unSafeMove.Key[1])
                    {
                        // Kontrollerar om egen pjäs redan har ett lagrat move
                        if (unSafeBeatMoves.ContainsKey(unSafeMove.Value))
                        {
                            // Om pjäsen redan har ett move kontrollera om det nya movet har högre värde än de gammla.
                            if (unSafeBeatMoves[unSafeMove.Value].Points < opponentPiece.Points)
                            {
                                unSafeBeatMoves[unSafeMove.Value] = opponentPiece;
                            }
                        }
                        else
                        {
                            unSafeBeatMoves.Add(unSafeMove.Value, opponentPiece);
                        }
                    }
                }

            }
        }
    
        public void BeatIfColide(int posX, int posY, Piece ownPiece, Player opponentPlayer )
        {

            // Om flyttad pjäs koliderar med motståndare slås denna ut.

            Piece beatenPiece = null;
            foreach (Piece opponentPiece in opponentPlayer.Pieces)
            {
                if (opponentPiece.PosX == posX && opponentPiece.PosY == posY)
                {
                    opponentPlayer.RemoveBeatenPiece(opponentPiece, ownPiece); // slår ut motståndarens pjäs 
                    beatenPiece = opponentPiece;
                    break;
                }
            }

            _ui.LoggLatestMove(ownPiece.PosX, ownPiece.PosY, posX, posY, ownPiece);
            if (beatenPiece != null)
            {
                _ui.LoggBeat(beatenPiece);
            }
            else
            {
                CalculatePawnMoves(ownPiece);
            }

            ownPiece.MovePiece(posX, posY); // Flyttar egen pjäs 
        }

        public bool IsKingSafeAfterMove(int newPosX, int newPosY, Piece ownPiece, Player currentPlayer, Player opponentPlayer)
        {
            // kontrollerar så att kungen är safe efter att pjäsen flyttats innan en flytt görs.

            int oldPosX = ownPiece.PosX;
            int oldPosY = ownPiece.PosY;

            ownPiece.MovePiece(newPosX, newPosY);

            if (currentPlayer.IsKingSafe(currentPlayer, opponentPlayer))
            {
                ownPiece.MovePiece(oldPosX, oldPosY);
                return true;
            }
            else
            {
                ownPiece.MovePiece(oldPosX, oldPosY);
                return false;
            }            
        }

        private bool MoveToThreatOpponentKing(Player currentPlayer, Player opponentPlayer)
        {

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    foreach (Piece ownPiece in currentPlayer.Pieces)
                    {
                        if (ownPiece.IsMoveValid(x,y,currentPlayer,opponentPlayer) && !IsKingSafeAfterMove(x,y,ownPiece,opponentPlayer,currentPlayer) && IsKingSafeAfterMove(x, y, ownPiece, currentPlayer, opponentPlayer) && !IsSquareThreaten(x,y,currentPlayer,opponentPlayer))
                        {
                            
                            BeatIfColide(x, y, ownPiece, opponentPlayer);
                            _ui.LoggCheck();
                            return true;
                        }
                    }
                }
            }

            return false;

            
        }
        public bool IsSquareThreaten(int PosX, int PosY, Player currentPlayer, Player opponentPlayer)
        {
            /*Kontrollerar om aktuell ruta är hotad av motståndaren, främst för kungens skull
            som aldrig får flytta till hotad ruta men skulle gå att använda för att förbättra AIn 
            Loopar igenom motståndarens pjäser för att se om vald ruta är ett valid move för denna.
            Separerat pawn och king från loopen med if satser för att undvika infinite loop på king
            samt eftersom pawn inte har "validMove" till rutor om där inte redan står en opponent.*/

            foreach (Piece piece in opponentPlayer.Pieces)
            {
                if(piece is Pawn)
                {
                    return ((Pawn) piece).IsThreatened(piece, PosX, PosY);
                }
                else if (piece.IsMoveValid(PosX, PosY, opponentPlayer, currentPlayer)) { return true; }

            }

            return false;
        }

        public void CalculatePawnMoves(Piece movedPiece)
        {
            // K : if (movedPiece.Type == "pawn") 
             if (movedPiece is Pawn)
            {
                _roundsWithoutPawnMove = 0;
            }
            else 
            {
                _roundsWithoutPawnMove++;
            }
        }

    }
}
