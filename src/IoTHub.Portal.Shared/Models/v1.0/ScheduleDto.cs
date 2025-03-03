// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    public class ScheduleDto
    {
        /// <summary>
        /// The level auto ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The schedule friendly name.
        /// </summary>
        public string Start { get; set; }
        /// <summary>
        /// Where schedule is.
        /// </summary>
        public string End { get; set; }
        /// <summary>
        /// The planning associat with the schedule.
        /// </summary>
        public string CommandId { get; set; }

        /// <summary>
        /// The planning associat with the schedule.
        /// </summary>
        public string PlanningId { get; set; }
    }
}
