// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    using System;

    //using System.ComponentModel.DataAnnotations;

    public class RoomDto
    {
        /// <summary>
        /// The room auto ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The room friendly name.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Where room is.
        /// </summary>
        public string Father { get; set; } = default!;

        /// <summary>
        /// The planning associat with the room.
        /// </summary>
        public string Planning { get; set; } = default!;
    }
}
