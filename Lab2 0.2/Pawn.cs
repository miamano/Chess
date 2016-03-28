using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2_0._2
{
    public class Pawn : Piece
    {
        public Pawn(int posX, int posY, string color) : base (posX, posY, color)
        {
            this.PosX = posX;
            this.PosY = posY;
            this.Color = color;
            this.Type = "pawn";
            this.Points = 1;
        }

        public override bool IsMoveValid(int newPosX, int newPosY, Player currentPlayer, Player opponentPlayer)
        {
            int startingRow;
            int endRow;
            int oneStep;
            int twoStep;

            if (Color == "white")
            {
                startingRow = 1;
                endRow = 7;
                oneStep = 1;
                twoStep = 2;
 
            }
            else // black
            {
                startingRow = 6;
                endRow = 0;
                oneStep = -1;
                twoStep = -2;
            }

            // Kontrollerar om pawn står på första raden, har då möjlighet att flytta 1 eller 2 steg.
            if (PosX == startingRow && PosY == newPosY)
            {
                bool tmpIsSquareClear = IsSquereClear(PosX + oneStep, PosY, currentPlayer) && IsSquereClear(PosX + oneStep, PosY, opponentPlayer);
                if (newPosX == PosX + oneStep && tmpIsSquareClear) { return true; }
                if (newPosX == PosX + twoStep && tmpIsSquareClear && IsSquereClear(PosX + twoStep, PosY, currentPlayer) && IsSquereClear(PosX + twoStep, PosY, opponentPlayer)) { return true; }
            }

            if (PosX != endRow)
            {
                if (newPosX == PosX + oneStep && newPosY == PosY && IsSquereClear(PosX + oneStep, PosY, currentPlayer) && IsSquereClear(PosX + oneStep, PosY, opponentPlayer)) { return true; }

                if (PosY < 7 && newPosX == PosX + oneStep && newPosY == PosY + 1 && IsSquereClear(PosX + oneStep, PosY + 1, currentPlayer) && !IsSquereClear(PosX + oneStep, PosY + 1, opponentPlayer)) { return true; }
                if (PosY > 0 && newPosX == PosX + oneStep && newPosY == PosY - 1 && IsSquereClear(PosX + oneStep, PosY - 1, currentPlayer) && !IsSquereClear(PosX + oneStep, PosY - 1, opponentPlayer)) { return true; }
            }

            return false;
        }

        public bool IsThreatened(Piece piece, int PosX, int PosY)
        {
            if (piece.PosX == PosX - 1 && piece.PosY == PosY - 1) { return true; }
            if (piece.PosX == PosX - 1 && piece.PosY == PosY + 1) { return true; }
            if (piece.PosX == PosX + 1 && piece.PosY == PosY + 1) { return true; }
            if (piece.PosX == PosX + 1 && piece.PosY == PosY - 1) { return true; }

            return false;
        }

    }
}
