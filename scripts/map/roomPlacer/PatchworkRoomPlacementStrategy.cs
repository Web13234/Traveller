﻿using System.Threading.Tasks;
using ColdMint.scripts.debug;
using ColdMint.scripts.levelGraphEditor;
using ColdMint.scripts.map.dateBean;
using ColdMint.scripts.map.interfaces;
using ColdMint.scripts.map.room;
using ColdMint.scripts.utils;
using Godot;

namespace ColdMint.scripts.map.RoomPlacer;

/// <summary>
/// <para>Patchwork room placement strategy</para>
/// <para>拼接的房间放置策略</para>
/// </summary>
/// <remarks>
///<para>Under this strategy, think of each room template as a puzzle piece, find their "slots", and then connect them together.</para>
///<para>在此策略下，将每个房间模板看作是一块拼图，找到他们的“槽”，然后将其连接在一起。</para>
/// </remarks>
public class PatchworkRoomPlacementStrategy : IRoomPlacementStrategy
{
    private readonly Vector2 _halfCell = new Vector2(Config.CellSize / 2f, Config.CellSize / 2f);

    public Task<bool> PlaceRoom(Node mapRoot, RoomPlacementData roomPlacementData)
    {
        if (roomPlacementData.Room == null || roomPlacementData.Position == null)
        {
            return Task.FromResult(false);
        }

        if (roomPlacementData.Room.RootNode == null)
        {
            return Task.FromResult(false);
        }

        var rootNode = roomPlacementData.Room.RootNode;
        mapRoot.AddChild(rootNode);
        rootNode.Position = roomPlacementData.Position.Value;
        return Task.FromResult(true);
    }

    public Task<RoomPlacementData?> CalculateNewRoomPlacementData(RandomNumberGenerator randomNumberGenerator,
        Room? parentRoomNode,
        RoomNodeData newRoomNodeData)
    {
        if (newRoomNodeData.RoomTemplateSet == null || newRoomNodeData.RoomTemplateSet.Length == 0)
        {
            return Task.FromResult<RoomPlacementData?>(null);
        }

        if (parentRoomNode == null)
        {
            return Task.FromResult<RoomPlacementData?>(null);
        }

        var roomResArray = RoomFactory.RoomTemplateSetToRoomRes(newRoomNodeData.RoomTemplateSet);
        if (roomResArray.Length == 0)
        {
            return Task.FromResult<RoomPlacementData?>(null);
        }

        var roomSlots = parentRoomNode.RoomSlots;
        if (roomSlots == null || roomSlots.Length == 0)
        {
            return Task.FromResult<RoomPlacementData?>(null);
        }

        //Matches unmatched slots.
        //对未匹配的插槽进行匹配。
        foreach (var roomRes in roomResArray)
        {
            var newRoom = RoomFactory.CreateRoom(roomRes);
            if (newRoom == null)
            {
                continue;
            }

            //Create a room, try to use the room slot to match the existing room slot.
            //创建了一个房间，尝试使用房间的槽与现有的房间槽匹配。
            if (!IsMatch(parentRoomNode, newRoom, out var mainRoomSlot, out var newRoomSlot).Result) continue;
            if (mainRoomSlot == null || newRoomSlot == null)
            {
                continue;
            }

            var position = CalculatedPosition(parentRoomNode, newRoom, mainRoomSlot, newRoomSlot, true)
                .Result;
            if (position == null) continue;
            var roomPlacementData = new RoomPlacementData
            {
                Room = newRoom,
                Position = position
            };
            return Task.FromResult<RoomPlacementData?>(roomPlacementData);
        }

        return Task.FromResult<RoomPlacementData?>(null);
    }

    public Task<RoomPlacementData?> CalculatePlacementDataForStartingRoom(RandomNumberGenerator randomNumberGenerator,
        RoomNodeData startRoomNodeData)
    {
        if (startRoomNodeData.RoomTemplateSet == null || startRoomNodeData.RoomTemplateSet.Length == 0)
        {
            return Task.FromResult<RoomPlacementData?>(null);
        }

        var roomResArray = RoomFactory.RoomTemplateSetToRoomRes(startRoomNodeData.RoomTemplateSet);
        if (roomResArray.Length == 0)
        {
            return Task.FromResult<RoomPlacementData?>(null);
        }

        var index = randomNumberGenerator.Randi() % roomResArray.Length;
        var roomPlacementData = new RoomPlacementData
        {
            Room = RoomFactory.CreateRoom(roomResArray[index]),
            Position = Vector2.Zero
        };
        return Task.FromResult<RoomPlacementData?>(roomPlacementData);
    }


    /// <summary>
    /// <para>if it matches</para>
    /// <para>是否匹配</para>
    /// </summary>
    /// <param name="mainRoom"></param>
    /// <param name="newRoom"></param>
    /// <returns></returns>
    public Task<bool> IsMatch(Room? mainRoom, Room newRoom, out RoomSlot? outMainRoomSlot, out RoomSlot? outNewRoomSlot)
    {
        if (mainRoom == null)
        {
            outNewRoomSlot = null;
            outMainRoomSlot = null;
            return Task.FromResult(false);
        }

        var roomSlots = mainRoom.RoomSlots;
        if (roomSlots == null)
        {
            outNewRoomSlot = null;
            outMainRoomSlot = null;
            return Task.FromResult(false);
        }

        var newRoomSlots = newRoom.RoomSlots;
        if (newRoomSlots == null)
        {
            outNewRoomSlot = null;
            outMainRoomSlot = null;
            return Task.FromResult(false);
        }

        foreach (var mainRoomSlot in roomSlots)
        {
            if (mainRoomSlot == null || mainRoomSlot.Matched)
            {
                //如果已经匹配过了，就不再匹配
                continue;
            }

            foreach (var newRoomSlot in newRoomSlots)
            {
                if (newRoomSlot == null)
                {
                    continue;
                }

                if (newRoomSlot.Matched)
                {
                    //如果已经匹配过了，就不再匹配
                    continue;
                }

                if (mainRoomSlot.IsHorizontal != newRoomSlot.IsHorizontal)
                {
                    continue;
                }

                if (mainRoomSlot.Length != newRoomSlot.Length)
                {
                    continue;
                }

                var distanceToMidpointOfRoom = mainRoomSlot.DistanceToMidpointOfRoom;
                var newDistanceToMidpointOfRoom = newRoomSlot.DistanceToMidpointOfRoom;
                if (distanceToMidpointOfRoom == null || newDistanceToMidpointOfRoom == null)
                {
                    continue;
                }

                if (distanceToMidpointOfRoom[0] == newDistanceToMidpointOfRoom[0] &&
                    distanceToMidpointOfRoom[1] == newDistanceToMidpointOfRoom[1])
                {
                    continue;
                }

                LogCat.Log(distanceToMidpointOfRoom[0] + "-" + distanceToMidpointOfRoom[1] + "和" +
                           newDistanceToMidpointOfRoom[0] + "-" + newDistanceToMidpointOfRoom[1] + "匹配成功");
                mainRoomSlot.Matched = true;
                newRoomSlot.Matched = true;
                outMainRoomSlot = mainRoomSlot;
                outNewRoomSlot = newRoomSlot;
                return Task.FromResult(true);
            }
        }

        outNewRoomSlot = null;
        outMainRoomSlot = null;
        return Task.FromResult(false);
    }

    private Task<Vector2?> CalculatedPosition(Room mainRoom, Room newRoom, RoomSlot? mainRoomSlot,
        RoomSlot? newRoomSlot, bool roomSlotOverlap)
    {
        if (mainRoom.RootNode == null || newRoom.RootNode == null || newRoom.TileMap == null ||
            mainRoom.TileMap == null ||
            newRoom.TileMap == null || mainRoomSlot == null ||
            newRoomSlot == null)
        {
            return Task.FromResult<Vector2?>(null);
        }

        //Main room slot location description
        //主房间槽位置描述
        var mainOrientationDescribe = mainRoomSlot.DistanceToMidpointOfRoom;
        //New room slot location description
        //新房间槽位置描述
        var newOrientationDescribe = newRoomSlot.DistanceToMidpointOfRoom;
        if (mainOrientationDescribe == null || newOrientationDescribe == null)
        {
            //If the room slot is described as null, null is returned
            //若房间槽描述为null，那么返回null
            return Task.FromResult<Vector2?>(null);
        }

        Vector2 result;
        if (mainOrientationDescribe[0] == CoordinateUtils.OrientationDescribe.Left &&
            newOrientationDescribe[0] == CoordinateUtils.OrientationDescribe.Right)
        {
            //Move left to new room.
            //左移新房间。
            var mainSlotPosition = mainRoom.RootNode.Position + mainRoom.TileMap.MapToLocal(mainRoomSlot.StartPosition);
            var newSlotPosition = newRoom.RootNode.Position + newRoom.TileMap.MapToLocal(newRoomSlot.StartPosition);
            result = mainSlotPosition +
                newRoom.TileMap.Position - newRoom.TileMap.MapToLocal(newRoomSlot.StartPosition);
            //Modified y height
            //修正y高度
            result.Y -= newSlotPosition.Y - mainSlotPosition.Y;
            //If the room slots don't overlap
            //如果房间槽不能重叠
            if (!roomSlotOverlap)
            {
                result.X -= Config.CellSize;
            }
        }
        else
        {
            var mainSlotPosition = mainRoom.RootNode.Position + mainRoom.TileMap.MapToLocal(mainRoomSlot.StartPosition);
            var newSlotPosition = newRoom.RootNode.Position + newRoom.TileMap.MapToLocal(newRoomSlot.StartPosition);
            result = mainSlotPosition;
            // result.Y += newSlotPosition.Y - mainSlotPosition.Y;
        }


        //We need to be on the same level.
        //我们需要在同一水平上。
        if (mainRoomSlot.IsHorizontal)
        {
            result += newRoom.TileMap.MapToLocal(new Vector2I(newRoomSlot.EndPosition.X, 0)) - _halfCell;
        }
        else
        {
            result -= newRoom.TileMap.MapToLocal(new Vector2I(0, newRoomSlot.EndPosition.Y)) - _halfCell;
        }

        return Task.FromResult<Vector2?>(result);
    }
}