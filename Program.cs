using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.OrTools.Algorithms;
using Google.OrTools.ConstraintSolver;
using System.IO;
using System.Collections;

namespace BinPacking
{
    class Program
    {
        int[] credits, prereq;
        int nbPeriods, nbCourses, minCredit, maxCredit, nbPrereqs;
        int binCount;
        Tuple<int, int>[] prereqTupleArr;

        private void Pack(Solver cp, IntVar[] binvars, int[] weights, IntVar[] loadvars)
        {
            IntVar[] b = new IntVar[binvars.Length];

            for(long j=0; j<loadvars.Length; j++)
            {
                for (int i = 0; i < binvars.Length; i++)
                {
                    b[i] = cp.MakeIsEqualCstVar(binvars[i], j);
                }
                cp.Add(cp.MakeScalProd(b, weights) == loadvars[j]);
            }
            
            cp.Add(cp.MakeSumEquality(loadvars, cp.MakeIntVar(weights.Sum(), weights.Sum(), "Sum")));
        }

        private void Readfile(string fileName)
        {
            StreamReader reader = new StreamReader(fileName);
            string line;

            line = reader.ReadLine();
            string[] token = line.Split(' ');
            nbCourses = Int32.Parse(token[0]);
            nbPeriods = Int32.Parse(token[1]);
            minCredit = Int32.Parse(token[2]);
            maxCredit = Int32.Parse(token[3]);
            nbPrereqs = Int32.Parse(token[4]);

            line = reader.ReadLine();
            token = line.Split();
            credits = new int[token.Length];
            int index = 0;
            foreach (string credit in token)
            {
                credits[index++] = Int32.Parse(credit);
            }

            line = reader.ReadLine();
            token = line.Split();
            prereq = new int[token.Length];
            index = 0;

            foreach (string st in token)
            {
                prereq[index++] = Int32.Parse(st);
            }

            Console.WriteLine("Prereq size: " + prereq.Length);
            prereqTupleArr = new Tuple<int, int>[nbPrereqs];
            for (int i = 0; i < nbPrereqs; i++)
            {
                prereqTupleArr[i] = Tuple.Create(prereq[i * 2], prereq[i * 2 + 1]);
            }

            reader.Close();
        }

        static void Main(string[] args)
        {
            Program obj = new Program();

            obj.Readfile(@"C:\binpackdata.txt");

            obj.nbCourses = obj.credits.Length;
            Solver solver = new Solver("BinPacking");

            IntVar[] x = new IntVar[obj.nbCourses];
            IntVar[] loadVars = new IntVar[obj.nbPeriods];

            for (int i = 0; i < obj.nbCourses; i++)
                x[i] = solver.MakeIntVar(0, obj.nbPeriods - 1, "x" + i);

            for (int i = 0; i < obj.nbPeriods; i++)
                loadVars[i] = solver.MakeIntVar(0, obj.credits.Sum(), "loadVars" + i);

            //-------------------post of the constraints--------------
            obj.Pack(solver, x, obj.credits, loadVars);

            foreach (Tuple<int, int> t in obj.prereqTupleArr)
                solver.Add(x[t.Item1] < x[t.Item2]);

            //-------------------------Objective---------------------------
            IntVar objectiveVar = solver.MakeMax(loadVars).Var();
            OptimizeVar objective = solver.MakeMinimize(objectiveVar, 1);

            //------------start the search and optimization-----------
            DecisionBuilder db = solver.MakePhase(x, Solver.CHOOSE_MIN_SIZE_LOWEST_MIN, Solver.INT_VALUE_DEFAULT);
            SearchMonitor searchLog = solver.MakeSearchLog(100000, objectiveVar);
            solver.NewSearch(db, objective, searchLog);

            while (solver.NextSolution())
            {
                Console.WriteLine(">> Objective: " + objectiveVar.Value());
            }

            solver.EndSearch();
        }
    }
}