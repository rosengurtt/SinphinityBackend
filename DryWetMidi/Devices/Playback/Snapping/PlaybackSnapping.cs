using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides a way to manage snap points for <see cref="Playback"/>.
    /// </summary>
    public sealed class PlaybackSnapping
    {
        #region Fields

        private readonly List<SnapPoint> _snapPoints = new List<SnapPoint>();

        private readonly IEnumerable<PlaybackEvent> _playbackEvents;
        private readonly TempoMap _tempoMap;
        private readonly TimeSpan _maxTime;

        private SnapPointsGroup _noteStartSnapPointsGroup;
        private SnapPointsGroup _noteEndSnapPointsGroup;

        #endregion

        #region Constructor

        internal PlaybackSnapping(IEnumerable<PlaybackEvent> playbackEvents, TempoMap tempoMap)
        {
            _playbackEvents = playbackEvents;
            _tempoMap = tempoMap;
            _maxTime = _playbackEvents.LastOrDefault()?.Time ?? TimeSpan.Zero;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets all snap points.
        /// </summary>
        public IEnumerable<SnapPoint> SnapPoints => _snapPoints.AsReadOnly();

        #endregion

        #region Methods

        /// <summary>
        /// Adds a snap point with the specified data at given time.
        /// </summary>
        /// <typeparam name="TData">Type of data that will be attached to a snap point.</typeparam>
        /// <param name="time">Time to add snap point at.</param>
        /// <param name="data">Data to attach to snap point.</param>
        /// <returns>An instance of the <see cref="SnapPoint{TData}"/> representing a snap point
        /// with <paramref name="data"/> at <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null.</exception>
        public SnapPoint<TData> AddSnapPoint<TData>(ITimeSpan time, TData data)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            var metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, _tempoMap);
            var snapPoint = new SnapPoint<TData>(metricTime, data);

            _snapPoints.Add(snapPoint);
            return snapPoint;
        }

        /// <summary>
        /// Adds a snap point at the specified time.
        /// </summary>
        /// <param name="time">Time to add snap point at.</param>
        /// <returns>An instance of the <see cref="SnapPoint{Guid}"/> representing a snap point
        /// at <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null.</exception>
        public SnapPoint<Guid> AddSnapPoint(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            return AddSnapPoint(time, Guid.NewGuid());
        }

        /// <summary>
        /// Removes a snap point.
        /// </summary>
        /// <typeparam name="TData">Type of data attached to <paramref name="snapPoint"/>.</typeparam>
        /// <param name="snapPoint">Snap point to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="snapPoint"/> is null.</exception>
        public void RemoveSnapPoint<TData>(SnapPoint<TData> snapPoint)
        {
            ThrowIfArgument.IsNull(nameof(snapPoint), snapPoint);

            _snapPoints.Remove(snapPoint);
        }

        /// <summary>
        /// Removes all snap points that match the conditions defined by the specified predicate.
        /// </summary>
        /// <typeparam name="TData">Type of data attached to snap points to remove.</typeparam>
        /// <param name="predicate">The <see cref="Predicate{TData}"/> delegate that defines the conditions
        /// of snap points to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
        public void RemoveSnapPointsByData<TData>(Predicate<TData> predicate)
        {
            ThrowIfArgument.IsNull(nameof(predicate), predicate);

            _snapPoints.RemoveAll(p =>
            {
                var snapPoint = p as SnapPoint<TData>;
                return snapPoint != null && predicate(snapPoint.Data);
            });
        }

        /// <summary>
        /// Adds snap points at times defined by the specified grid.
        /// </summary>
        /// <param name="grid">The grid that defines times to add snap points to.</param>
        /// <returns>An instance of the <see cref="SnapPointsGroup"/> added snap points belong to.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="grid"/> is null.</exception>
        public SnapPointsGroup SnapToGrid(IGrid grid)
        {
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var snapPointsGroup = new SnapPointsGroup();

            foreach (var time in grid.GetTimes(_tempoMap))
            {
                TimeSpan metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, _tempoMap);
                if (metricTime > _maxTime)
                    break;

                _snapPoints.Add(new SnapPoint(metricTime) { SnapPointsGroup = snapPointsGroup });
            }

            return snapPointsGroup;
        }

        /// <summary>
        /// Adds snap points at start times of notes.
        /// </summary>
        /// <returns>An instance of the <see cref="SnapPointsGroup"/> added snap points belong to.</returns>
        public SnapPointsGroup SnapToNotesStarts()
        {
            return _noteStartSnapPointsGroup ?? (_noteStartSnapPointsGroup = SnapToNoteEvents(snapToNoteOn: true));
        }

        /// <summary>
        /// Adds snap points at end times of notes.
        /// </summary>
        /// <returns>An instance of the <see cref="SnapPointsGroup"/> added snap points belong to.</returns>
        public SnapPointsGroup SnapToNotesEnds()
        {
            return _noteEndSnapPointsGroup ?? (_noteEndSnapPointsGroup = SnapToNoteEvents(snapToNoteOn: false));
        }

        internal SnapPoint GetNextSnapPoint(TimeSpan time, SnapPointsGroup snapPointsGroup)
        {
            return GetActiveSnapPoints(snapPointsGroup).SkipWhile(p => p.Time <= time).FirstOrDefault();
        }

        internal SnapPoint GetNextSnapPoint(TimeSpan time)
        {
            return GetActiveSnapPoints().SkipWhile(p => p.Time <= time).FirstOrDefault();
        }

        internal SnapPoint GetPreviousSnapPoint(TimeSpan time, SnapPointsGroup snapPointsGroup)
        {
            return GetActiveSnapPoints(snapPointsGroup).TakeWhile(p => p.Time < time).LastOrDefault();
        }

        internal SnapPoint GetPreviousSnapPoint(TimeSpan time)
        {
            return GetActiveSnapPoints().TakeWhile(p => p.Time < time).LastOrDefault();
        }

        private SnapPointsGroup SnapToNoteEvents(bool snapToNoteOn)
        {
            var times = new List<ITimeSpan>();

            foreach (var playbackEvent in _playbackEvents)
            {
                var noteMetadata = playbackEvent.Metadata.Note;
                if (noteMetadata == null || (playbackEvent.Event is NoteOnEvent) != snapToNoteOn)
                    continue;

                times.Add((MetricTimeSpan)playbackEvent.Time);
            }

            return SnapToGrid(new ArbitraryGrid(times));
        }

        private IEnumerable<SnapPoint> GetActiveSnapPoints()
        {
            return _snapPoints.Where(p => p.IsEnabled && p.SnapPointsGroup?.IsEnabled != false).OrderBy(p => p.Time);
        }

        private IEnumerable<SnapPoint> GetActiveSnapPoints(SnapPointsGroup snapPointsGroup)
        {
            return GetActiveSnapPoints().Where(p => p.SnapPointsGroup == snapPointsGroup);
        }

        #endregion
    }
}
