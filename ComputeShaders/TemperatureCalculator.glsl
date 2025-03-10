#[compute]

#version 450

layout(local_size_x=32,local_size_y=32,local_size_z=1)in;

layout(set=0,binding=0,std430)buffer LocalTemp{
    float data[];
}local_temp;

layout(set=0,binding=1,std430)buffer NeighborIndex{
    uvec4 data[];
}neighbor_index;

layout(set=0,binding=2,std430)buffer DeltaTime{
    float timestamp;
}delta_time;

layout(set=0,binding=3,std430)buffer Alpha{
    float data;
}alpha;

layout(set=0,binding=4,std430)buffer FaceLength{
    uint data;
}face_length;


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
    
    deltaT+=delta_time.timestamp*(local_temp.data[neighbor_index.data[id].x]-local_temp.data[id]*.8);
    deltaT+=delta_time.timestamp*(local_temp.data[neighbor_index.data[id].y]-local_temp.data[id]*.8);
    deltaT+=delta_time.timestamp*(local_temp.data[neighbor_index.data[id].z]-local_temp.data[id]*.8);
    deltaT+=delta_time.timestamp*(local_temp.data[neighbor_index.data[id].w]-local_temp.data[id]*.8);
    
    local_temp.data[id]+=deltaT;
}
