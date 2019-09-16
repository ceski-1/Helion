using Helion.Bsp.Geometry;
using Helion.Bsp.States.Split;
using static Helion.Util.Assertion.Assert;

namespace Helion.Bsp.Impl.Debuggable
{
    public class DebuggableSplitCalculator : SplitCalculator
    {
        public DebuggableSplitCalculator(BspConfig bspConfig) : base(bspConfig)
        {
        }

        public override void Execute()
        {
            Precondition(States.State != SplitterState.Finished, "Trying to run a split checker when finished");
            Precondition(States.CurrentSegmentIndex < States.Segments.Count, "Out of range split calculator segment index");

            BspSegment splitter = States.Segments[States.CurrentSegmentIndex];
            States.CurrentSegScore = CalculateScore(splitter);

            if (States.CurrentSegScore < States.BestSegScore)
            {
                Invariant(!splitter.IsMiniseg, "Should never be selecting a miniseg as a splitter");
                States.BestSegScore = States.CurrentSegScore;
                States.BestSplitter = splitter;
            }

            States.CurrentSegmentIndex++;

            bool hasSegmentsLeft = States.CurrentSegmentIndex < States.Segments.Count;
            States.State = (hasSegmentsLeft ? SplitterState.Working : SplitterState.Finished);
        }
    }
}