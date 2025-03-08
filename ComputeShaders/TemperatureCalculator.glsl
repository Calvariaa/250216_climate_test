

#[compute]

#version 450

layout(local_size_x=32,local_size_y=32,local_size_z=1)in;

layout(set=0,binding=0,std430)buffer LocalTemp{
    float data[];
}local_temp;

layout(set=0,binding=1,std430)buffer NeighborIndex{
    float data[][];
}neighbor_index;

enum Direction
{
    Up=0,
    Down,
    Left,
    Right
}

void main()
{
    uint x=gl_GlobalInvocationID.x;
    uint y=gl_GlobalInvocationID.y;
    uint z=gl_GlobalInvocationID.z;
    
    uint id=gl_GlobalInvocationID.x+
    gl_GlobalInvocationID.y*(gl_NumWorkGroups.x*gl_WorkGroupSize.x)+
    gl_GlobalInvocationID.z*(gl_NumWorkGroups.x*gl_WorkGroupSize.x)*
    (gl_NumWorkGroups.y*gl_WorkGroupSize.y);
    
    float deltaT=0.;
    
    deltaT+=(local_temp.data[neighbor_index.data[id][Up]]-local_temp.data[id])*.1;
    deltaT+=(local_temp.data[neighbor_index.data[id][Down]]-local_temp.data[id])*.1;
    deltaT+=(local_temp.data[neighbor_index.data[id][Left]]-local_temp.data[id])*.1;
    deltaT+=(local_temp.data[neighbor_index.data[id][Right]]-local_temp.data[id])*.1;
    
    local_temp.data[id]+=deltaT;
    
}
