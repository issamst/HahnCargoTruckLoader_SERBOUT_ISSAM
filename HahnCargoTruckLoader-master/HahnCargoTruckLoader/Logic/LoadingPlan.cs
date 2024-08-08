using HahnCargoTruckLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HahnCargoTruckLoader.Logic
{
    public class LoadingPlan
    {
        private readonly Dictionary<int, LoadingInstruction> instructions;

        public LoadingPlan(Truck truck, List<Crate> crates)
        {
            instructions = new Dictionary<int, LoadingInstruction>();

            bool[,,] cargoSpace = new bool[truck.Width, truck.Height, truck.Length];

            int step = 1;
            foreach (var crate in crates)
            {
                Console.WriteLine($"Trying to place crate {crate.CrateID}");
                var (pos, turns) = FindValidPosition(crate, cargoSpace);

                if (pos != null)
                {
                    PlaceCrate(crate, cargoSpace, pos, turns);

                    var instruction = new LoadingInstruction
                    {
                        LoadingStepNumber = step++,
                        CrateId = crate.CrateID,
                        TopLeftX = pos.x,
                        TopLeftY = pos.y,
                        TurnHorizontal = turns.horizontal,
                        TurnVertical = turns.vertical
                    };

                    instructions.Add(crate.CrateID, instruction);
                    Console.WriteLine($"Placed crate {crate.CrateID} at position ({pos.x}, {pos.y}, {pos.z}) with turns (horizontal: {turns.horizontal}, vertical: {turns.vertical})");
                }
                else
                {
                    throw new InvalidOperationException($"Could not place crate {crate.CrateID}");
                }
            }
        }

        public Dictionary<int, LoadingInstruction> GetLoadingInstructions()
        {
            return instructions;
        }
        private (Position?, (bool horizontal, bool vertical)) FindValidPosition(Crate crate, bool[,,] cargoSpace)
        {
            var orientations = new[]
            {
        (false, false),
        (true, false),
        (false, true),
        (true, true)
    };

            for (int x = 0; x < cargoSpace.GetLength(0); x++)
            {
                for (int y = 0; y < cargoSpace.GetLength(1); y++)
                {
                    for (int z = 0; z < cargoSpace.GetLength(2); z++)
                    {
                        foreach (var (horizontal, vertical) in orientations)
                        {
                            crate.Turn(new LoadingInstruction { TurnHorizontal = horizontal, TurnVertical = vertical });

                            if (CanPlaceCrate(crate, cargoSpace, x, y, z))
                            {
                                return (new Position(x, y, z), (horizontal, vertical));
                            }
                            else
                            {
                            }

                            crate.Turn(new LoadingInstruction { TurnHorizontal = horizontal, TurnVertical = vertical }); // revert rotation
                        }
                    }
                }
            }

            return (null, (false, false));
        }

        private bool CanPlaceCrate(Crate crate, bool[,,] cargoSpace, int x, int y, int z)
        {
            if (x + crate.Width > cargoSpace.GetLength(0) ||
                y + crate.Height > cargoSpace.GetLength(1) ||
                z + crate.Length > cargoSpace.GetLength(2))
            {
                return false;
            }

            for (int i = x; i < x + crate.Width; i++)
            {
                for (int j = y; j < y + crate.Height; j++)
                {
                    for (int k = z; k < z + crate.Length; k++)
                    {
                        if (cargoSpace[i, j, k]) return false;
                    }
                }
            }

            return true;
        }

        private void PlaceCrate(Crate crate, bool[,,] cargoSpace, Position pos, (bool horizontal, bool vertical) turns)
        {
            for (int i = pos.x; i < pos.x + crate.Width; i++)
            {
                for (int j = pos.y; j < pos.y + crate.Height; j++)
                {
                    for (int k = pos.z; k < pos.z + crate.Length; k++)
                    {
                        cargoSpace[i, j, k] = true;
                    }
                }
            }
        }

        private record Position(int x, int y, int z);
    }
}
